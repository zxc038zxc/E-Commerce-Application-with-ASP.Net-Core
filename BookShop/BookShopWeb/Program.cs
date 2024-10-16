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

// �ۭq ASP.NET Core Identity �{�Ҩt�Τ����ε{�� Cookie ���欰
// �]�w���{�Ҫ��ϥΪ̹��զs�������귽�ɡA���Q���w�V�쪺�n�J�������|�C
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

// ���U�������O����֨��A�ȡC���F�x�s���ε{����ƪ��֨�����A�Y�����ε{�����ݦb�h�Ӧ��A���]��O���@�P
// AddSession�A���\�Τ��s�������x�s�M�޲z��ơF�z�LSession�A�i�H�b�h��Http�ШD�����x�s�Τ��ơC�o�Ǹ�Ʒ|�b���A���ݦs�x�A�åH�@�Ӱߤ@��SessionID����
// IdltTimeout�ASession���m�ɶ��A�Y�W�L100�����S�����ʡASession��Ʒ|�۰ʹL���çR��
// Cookie�u��z�LHTTPS�ШD�i��s���A�L�k�bJS���X�ݡA�W�[�w���ʨ���XSS����
// �]�wCookie�O�������A�Y�ϥΤ�ڵ��F�D���n��Cookie�A�o��Cookie�̵M�|�Q�s���A�]���������ε{�����\��O������
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
// �������ҥ���������v
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();
