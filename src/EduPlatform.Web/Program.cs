using EduPlatform.Core.Entities;
using EduPlatform.Core.Interfaces;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Infrastructure.Services;
using EduPlatform.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===========================================
// 1. إضافة DbContext (قاعدة البيانات)
// ===========================================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===========================================
// 2. إضافة Identity (نظام المستخدمين)
// ===========================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // إعدادات كلمة السر
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // إعدادات قفل الحساب
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // إعدادات المستخدم
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// ===========================================
// 3. إضافة Services
// ===========================================
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Services الخاصة بالمشروع
builder.Services.AddScoped<IEnrollmentService, EduPlatform.Infrastructure.Services.EnrollmentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITicketService, TicketService>();

// ===========================================
// 4. إضافة Session (دعم الجلسات)
// ===========================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ✅ كل Services لازم تضاف قبل builder.Build()

var app = builder.Build(); // ✅ هنا فقط نعمل Build

// ===========================================
// 5. إعداد Middleware (الـ Pipeline)
// ===========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// ✅ Session لازم قبل Authentication
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Routes للـ Areas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Route الافتراضي
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<EduPlatform.Web.Hubs.NotificationHub>("/notificationHub");

// ===========================================
// 6. تهيئة قاعدة البيانات (Seeding)
// ===========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await EduPlatform.Infrastructure.Data.DbInitializer.Initialize(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();