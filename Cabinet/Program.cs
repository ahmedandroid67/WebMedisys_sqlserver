using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
        policy.RequireRole("Admin", "Medecin"));
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
        options.Cookie.SameSite = SameSiteMode.Lax;
        // Bug 1 fix: Always in production, SameAsRequest in development (HTTP)
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
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
app.UseRouting();

// Middleware order is critical
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
