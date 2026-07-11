using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Cabinet.Data;

var builder = WebApplication.CreateBuilder(args);

// In Program.cs
builder.Services.AddRazorPages(options =>
{
    // Protect the root folder and all subfolders
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AuthorizeFolder("/Reports", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Employers", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Medicaments", "AdminOnly");
    options.Conventions.AuthorizeFolder("/services", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Stock", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Settings", "AdminOnly");

    // If you have a specific Login page that must be public:
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Account/AccessDenied");
});

builder.Services.AddMemoryCache();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin", "Medecin")
              .RequireClaim("CanAccessAdmin", "True"));
});

// Configuration of the Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Correct path to your folder
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "CabinetAuth";
        options.Cookie.SameSite = SameSiteMode.Strict;
        // Bug 1 fix: Always in production, SameAsRequest in development (HTTP)
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;

        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var email = context.Principal?.Identity?.Name;
                if (string.IsNullOrEmpty(email))
                {
                    context.RejectPrincipal();
                    return;
                }

                var scope = context.HttpContext.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var user = await db.Employer
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Email == email);
                scope.Dispose();

                if (user == null)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                var currentRole = context.Principal.FindFirstValue(ClaimTypes.Role);
                var currentAdminClaim = context.Principal.FindFirstValue("CanAccessAdmin");
                var currentEmployerId = context.Principal.FindFirstValue("EmployerId");

                if (currentRole != user.Role
                    || currentAdminClaim != user.CanAccessAdmin.ToString()
                    || currentEmployerId != user.IdEmployer.ToString())
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim("FullName", $"{user.Nom} {user.Prenom}"),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("CanAccessAdmin", user.CanAccessAdmin.ToString()),
                        new Claim("EmployerId", user.IdEmployer.ToString())
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    context.ReplacePrincipal(new ClaimsPrincipal(identity));
                    context.ShouldRenew = true;
                }
            }
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Ensure the application uses French culture for dates, numbers, etc. globally.
var supportedCultures = new[] { "fr-FR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

app.UseHttpsRedirection();
app.UseStaticFiles();

// Security headers
app.Use(async (ctx, next) =>
{
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
    ctx.Response.Headers["X-Frame-Options"] = "DENY";
    ctx.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    ctx.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});
app.UseRouting();

// Middleware order is critical
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
