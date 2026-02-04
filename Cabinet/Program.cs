using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Cabinet.Data;

var builder = WebApplication.CreateBuilder(args);

// In Program.cs
builder.Services.AddRazorPages(options =>
{
    // Protect the root folder and all subfolders
    options.Conventions.AuthorizeFolder("/");

    // If you have a specific Login page that must be public:
    options.Conventions.AllowAnonymousToPage("/Account/Login");
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuration of the Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Correct path to your folder
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
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