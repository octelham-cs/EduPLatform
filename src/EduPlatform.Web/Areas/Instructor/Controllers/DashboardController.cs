// src/EduPlatform.Web/Areas/Instructor/Controllers/DashboardController.cs

using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Web.ViewModels.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EduPlatform.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null)
            {
                TempData["Error"] = "لم يتم العثور على حساب المدرس";
                return RedirectToAction("Index", "Home");
            }

            var courses = await _context.Courses
                .Where(c => c.InstructorId == instructor.Id)
                .ToListAsync();

            var courseIds = courses.Select(c => c.Id).ToList();

            // إحصائيات
            var totalVideos = await _context.Videos
                .Include(v => v.Chapter)
                .CountAsync(v => courseIds.Contains(v.Chapter.CourseId));

            var codes = await _context.EnrollmentCodes
                .Where(e => e.InstructorId == instructor.Id)
                .ToListAsync();

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => courseIds.Contains(e.CourseId))
                .CountAsync();

            var recentEnrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s!.User)
                .Include(e => e.Course)
                .Where(e => courseIds.Contains(e.CourseId))
                .OrderByDescending(e => e.EnrolledAt)
                .Take(5)
                .ToListAsync();

            var recentCodes = codes
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .ToList();

            var viewModel = new InstructorDashboardViewModel
            {
                TotalCourses = courses.Count,
                TotalVideos = totalVideos,
                TotalStudents = enrollments,
                ActiveCodes = codes.Count(c => c.Status == CodeStatus.Available),
                AvailableCodes = codes.Count(c => c.Status == CodeStatus.Available),
                UsedCodes = codes.Count(c => c.Status == CodeStatus.Used),
                RecentStudents = recentEnrollments.Select(e => new RecentStudentViewModel
                {
                    Name = e.Student?.User?.FullName ?? "غير معروف",
                    Course = e.Course?.Title ?? "",
                    EnrolledAt = e.EnrolledAt
                }).ToList(),
                RecentCodes = recentCodes.Select(c => new RecentCodeViewModel
                {
                    Code = c.Code,
                    Status = c.Status switch
                    {
                        CodeStatus.Available => "متاح",
                        CodeStatus.Used => "مستخدم",
                        CodeStatus.Expired => "منتهي",
                        _ => "غير معروف"
                    },
                    CreatedAt = c.CreatedAt
                }).ToList()
            };

            return View(viewModel);
        }
    }
}