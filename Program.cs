using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using okem_social.Data;
using okem_social.Models;

// 👇 Thêm các using cho DI
using okem_social.Repositories;
using okem_social.Services;


var builder = WebApplication.CreateBuilder(args);

// EF Core + SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cookie Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccessDenied";
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.SlidingExpiration = true;
        // o.Cookie.Name = "okem_auth"; // (tuỳ chọn)
    });

builder.Services.AddControllersWithViews();

// 👇 Đăng ký Repository/Service cho phần đăng nhập
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Auto-migrate + seed 1 admin
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any(u => u.Role == Role.Admin))
    {
        db.Users.Add(new User
        {
            Email = "admin@okem.vn",
            FullName = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin!12345"),
            Role = Role.Admin
        });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();     // ✅ phải trước Authorization
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
