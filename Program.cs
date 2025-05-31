using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebBanMayTinh.Models;
using WebBanMayTinh.Repositories;
using WebBanMayTinh.Services;
using WebBanMayTinh.Models.VNPAY;
using FluentAssertions.Common;


var builder = WebApplication.CreateBuilder(args);


// Add Controllers with Views
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IRevenueService, RevenueService>();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Thêm retry logic để chờ database sẵn sàng
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

// Add Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddDefaultUI()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
});
//dang ky vnpay
builder.Services.AddSingleton<IVnPayService, VnPayService>();
// Add Razor Pages for Identity UI
builder.Services.AddRazorPages();

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Repositories
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IReviewRepository, EFReviewRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();

// Thêm cấu hình Data Protection (loại bỏ cảnh báo)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/root/.aspnet/DataProtection-Keys"))
    .SetApplicationName("WebBanMayTinh");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})



// Cấu hình cổng lắng nghe (khớp với docker-compose.yml)
//builder.WebHost.UseUrls("http://[::]:80");   // khi nào build trên docker thì mở, ko thì cmt nó lại ko xảy ra lỗi build trên trình biên dịch
.AddCookie()  // Lưu trạng thái đăng nhập
.AddGoogle(options =>
{
    options.ClientId = "327881816650-htqklrhcm4ob9jbtki12kiv206q3jeqt.apps.googleusercontent.com";
    options.ClientSecret = "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";
    options.CallbackPath = "/signin-google"; // Đường dẫn chuyển hướng
})

    .AddFacebook(options =>
    {
        options.AppId = "YOUR_FACEBOOK_APP_ID";
        options.AppSecret = "YOUR_FACEBOOK_APP_SECRET";
    });


var app = builder.Build();


// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();




app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();