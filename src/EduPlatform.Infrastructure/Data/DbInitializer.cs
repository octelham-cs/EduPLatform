using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // 1. إنشاء الأدوار
            string[] roles = { "Admin", "Instructor", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. إنشاء Admin
            var adminEmail = "admin@edu.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "المسؤول الرئيسي",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // 3. إنشاء بيانات أساسية (المستويات والمواد)
            if (!context.GradeLevels.Any())
            {
                var grade1 = new GradeLevel { Name = "الصف الأول الثانوي" };
                var grade2 = new GradeLevel { Name = "الصف الثاني الثانوي" };
                context.GradeLevels.AddRange(grade1, grade2);
                await context.SaveChangesAsync();

                var math = new Subject { Name = "الرياضيات", GradeLevelId = grade1.Id };
                var arabic = new Subject { Name = "اللغة العربية", GradeLevelId = grade1.Id };
                context.Subjects.AddRange(math, arabic);
                await context.SaveChangesAsync();
            }

            // 4. إنشاء مدرس تجريبي
            var instructorEmail = "teacher@edu.com";
            if (await userManager.FindByEmailAsync(instructorEmail) == null)
            {
                var instructorUser = new ApplicationUser
                {
                    UserName = instructorEmail,
                    Email = instructorEmail,
                    FullName = "أ/ محمد أحمد",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(instructorUser, "Teacher@123");
                await userManager.AddToRoleAsync(instructorUser, "Instructor");

                var instructor = new Instructor
                {
                    UserId = instructorUser.Id,
                    Bio = "مدرس رياضيات متميز",
                    Status = InstructorStatus.Approved // تم الموافقة عليه
                };
                context.Instructors.Add(instructor);
                await context.SaveChangesAsync();

                // إضافة كورس له
                var subject = context.Subjects.First();
                var course = new Course
                {
                    Title = "مaths Course",
                    Description = "شرح كامل للرياضيات",
                    InstructorId = instructor.Id,
                    SubjectId = subject.Id,
                    Price = 100,
                    CreatedAt = DateTime.UtcNow
                };
                context.Courses.Add(course);
                await context.SaveChangesAsync();
            }

            // 5. إنشاء طالب تجريبي
            var studentEmail = "student@edu.com";
            if (await userManager.FindByEmailAsync(studentEmail) == null)
            {
                var studentUser = new ApplicationUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    FullName = "علي الطالب",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(studentUser, "Student@123");
                await userManager.AddToRoleAsync(studentUser, "Student");

                var grade = context.GradeLevels.First();
                var student = new Student
                {
                    UserId = studentUser.Id,
                    GradeLevelId = grade.Id
                };
                context.Students.Add(student);
                await context.SaveChangesAsync();
            }
        }
    }
}