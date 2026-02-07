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
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Middleware order is critical
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.Run();
