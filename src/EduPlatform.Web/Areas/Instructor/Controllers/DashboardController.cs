using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using EduPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ========================================
        // GET: Instructor/Dashboard
        // ========================================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null)
            {
                return NotFound();
            }

            // لو المدرس موافق عليه
            if (instructor.Status == InstructorStatus.Approved)
            {
                // إحصائيات المدرس
                ViewBag.TotalCourses = await _context.Courses
                    .CountAsync(c => c.InstructorId == instructor.Id);

                ViewBag.TotalStudents = 0; // هنحسبها لاحقاً

                // الكورسات
                var courses = await _context.Courses
                    .Include(c => c.Subject)
                    .Where(c => c.InstructorId == instructor.Id)
                    .ToListAsync();

                ViewBag.Courses = courses;
            }

            return View(instructor);
        }
    }
}