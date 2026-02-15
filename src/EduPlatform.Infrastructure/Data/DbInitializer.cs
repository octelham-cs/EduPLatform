using EduPlatform.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // تأكد إن قاعدة البيانات موجودة
            await context.Database.MigrateAsync();

            // 1. إنشاء الأدوار (Roles)
            await CreateRolesAsync(roleManager);

            // 2. إنشاء الأدمن الافتراضي
            await CreateAdminAsync(userManager);

            // 3. إضافة المستويات الدراسية
            await SeedGradeLevelsAsync(context);

            // 4. إضافة الشعب
            await SeedBranchesAsync(context);

            // 5. إضافة الاترمة
            await SeedAcademicTermsAsync(context);
        }

        // ========================================
        // 1. إنشاء الأدوار
        // ========================================
        private static async Task CreateRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Instructor", "Student" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        // ========================================
        // 2. إنشاء الأدمن الافتراضي
        // ========================================
        private static async Task CreateAdminAsync(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "admin@eduplatform.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "المسؤول الرئيسي",
                    PhoneNumber = "01000000000",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        // ========================================
        // 3. إضافة المستويات الدراسية
        // ========================================
        private static async Task SeedGradeLevelsAsync(ApplicationDbContext context)
        {
            if (!await context.GradeLevels.AnyAsync())
            {
                var gradeLevels = new List<GradeLevel>
                {
                    new GradeLevel { Name = "أولى ثانوي", NameEn = "First Year", Order = 1, IsActive = true },
                    new GradeLevel { Name = "ثانية ثانوي", NameEn = "Second Year", Order = 2, IsActive = true },
                    new GradeLevel { Name = "ثالثة ثانوي", NameEn = "Third Year", Order = 3, IsActive = true }
                };

                await context.GradeLevels.AddRangeAsync(gradeLevels);
                await context.SaveChangesAsync();
            }
        }

        // ========================================
        // 4. إضافة الشعب
        // ========================================
        private static async Task SeedBranchesAsync(ApplicationDbContext context)
        {
            if (!await context.Branches.AnyAsync())
            {
                var branches = new List<Branch>
                {
                    new Branch { Name = "علمي علوم", NameEn = "Science", IsActive = true },
                    new Branch { Name = "علمي رياضة", NameEn = "Math", IsActive = true },
                    new Branch { Name = "أدبي", NameEn = "Literary", IsActive = true }
                };

                await context.Branches.AddRangeAsync(branches);
                await context.SaveChangesAsync();
            }
        }



        // ========================================
        // 5. إضافة الأترمة الدراسية
        // ========================================
        private static async Task SeedAcademicTermsAsync(ApplicationDbContext context)
        {
            if (!await context.AcademicTerms.AnyAsync())
            {
                var currentYear = DateTime.Now.Year;

                var terms = new List<AcademicTerm>
        {
            new AcademicTerm
            {
                Name = $"الترم الأول {currentYear}",
                Year = currentYear,
                StartDate = new DateTime(currentYear, 9, 1),
                EndDate = new DateTime(currentYear + 1, 1, 31),
                IsActive = true
            },
            new AcademicTerm
            {
                Name = $"الترم الثاني {currentYear}",
                Year = currentYear,
                StartDate = new DateTime(currentYear + 1, 2, 1),
                EndDate = new DateTime(currentYear + 1, 6, 30),
                IsActive = true
            }
        };

                await context.AcademicTerms.AddRangeAsync(terms);
                await context.SaveChangesAsync();
            }
        }
    }
}