using BookShop.DataAccess.Data;
using BookShop.DataAccess.Repository;
using BookShop.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using BookShop.Utility;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// 自訂 ASP.NET Core Identity 認證系統中應用程式 Cookie 的行為
// 設定當未認證的使用者嘗試存取受限資源時，應被重定向到的登入頁面路徑。
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

builder.Services.AddAuthentication().AddFacebook(option =>
{
    option.AppId = "532467459511221";
    option.AppSecret = "ca2542ac4a681b3ce92cecfc38883e3b";
});

// 註冊分散式記憶體快取服務。為了儲存應用程式資料的快取機制，即使應用程式部屬在多個伺服器也能保持一致
// AddSession，允許用戶瀏覽期間儲存和管理資料；透過Session，可以在多個Http請求之間儲存用戶資料。這些資料會在伺服器端存儲，並以一個唯一的SessionID辨識
// IdltTimeout，Session閒置時間，若超過100分鐘沒有活動，Session資料會自動過期並刪除
// Cookie只能透過HTTPS請求進行存取，無法在JS中訪問，增加安全性防止XSS攻擊
// 設定Cookie是必須的，即使用戶拒絕了非必要的Cookie，這個Cookie依然會被存取，因為它對應用程式的功能是必須的
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddRazorPages();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();
app.UseRouting();
// 身分驗證必須先於授權
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
