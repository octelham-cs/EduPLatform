using EduPlatform.Core.Entities;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
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
        // GET: Admin/Dashboard
        // ========================================
        public async Task<IActionResult> Index()
        {
            // إحصائيات عامة
            ViewBag.TotalInstructors = await _context.Instructors.CountAsync();
            ViewBag.PendingInstructors = await _context.Instructors
                .CountAsync(i => i.Status == InstructorStatus.Pending);
            ViewBag.TotalStudents = await _context.Students.CountAsync();
            ViewBag.TotalCourses = await _context.Courses.CountAsync();

            // قائمة المدرسين المعلقين
            var pendingInstructors = await _context.Instructors
                .Include(i => i.User)
                .Where(i => i.Status == InstructorStatus.Pending)
                .OrderByDescending(i => i.RegisteredAt)
                .Take(10)
                .ToListAsync();

            return View(pendingInstructors);
        }

        // ========================================
        // GET: Admin/Instructors
        // ========================================
        public async Task<IActionResult> Instructors()
        {
            var instructors = await _context.Instructors
                .Include(i => i.User)
                .OrderByDescending(i => i.RegisteredAt)
                .ToListAsync();

            return View(instructors);
        }

        // ========================================
        // POST: Admin/ApproveInstructor/5
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveInstructor(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);

            if (instructor == null)
            {
                return NotFound();
            }

            instructor.Status = InstructorStatus.Approved;
            instructor.ApprovedAt = DateTime.Now;
            instructor.ApprovedBy = _userManager.GetUserId(User);

            await _context.SaveChangesAsync();

            TempData["Success"] = "تمت الموافقة على المدرس بنجاح";
            return RedirectToAction(nameof(Instructors));
        }

        // ========================================
        // POST: Admin/RejectInstructor/5
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectInstructor(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);

            if (instructor == null)
            {
                return NotFound();
            }

            instructor.Status = InstructorStatus.Rejected;
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم رفض المدرس";
            return RedirectToAction(nameof(Instructors));
        }

        // ========================================
        // GET: Admin/Students
        // ========================================
        public async Task<IActionResult> Students()
        {
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.GradeLevel)
                .OrderByDescending(s => s.RegisteredAt)
                .ToListAsync();

            return View(students);
        }

        // ========================================
        // GET: Admin/Subjects
        // ========================================
        public async Task<IActionResult> Subjects()
        {
            var subjects = await _context.Subjects
                .Include(s => s.GradeLevelId)
                .OrderBy(s => s.GradeLevelId)
                .ToListAsync();

            return View(subjects);
        }

        // ========================================
        // GET: Admin/AcademicTerms
        // ========================================
        public async Task<IActionResult> AcademicTerms()
        {
            var terms = await _context.AcademicTerms
                .OrderByDescending(t => t.Year)
                .ToListAsync();

            return View(terms);
        }


        // ========================================
        // GET: Admin/AssignInstructor
        // ========================================
        [HttpGet]
        public async Task<IActionResult> AssignInstructor()
        {
            // جلب المستخدمين اللي مش مدرسين
            var instructorUserIds = await _context.Instructors
                .Select(i => i.UserId)
                .ToListAsync();

            var users = await _userManager.Users
                .Where(u => !instructorUserIds.Contains(u.Id))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            return View(users);
        }

        // ========================================
        // POST: Admin/AssignInstructor
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignInstructor(string userId, string? bio)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] = "المستخدم غير موجود";
                return RedirectToAction(nameof(AssignInstructor));
            }

            // التحقق إن المستخدم مش مدرس بالفعل
            var existingInstructor = await _context.Instructors
                .AnyAsync(i => i.UserId == userId);

            if (existingInstructor)
            {
                TempData["Error"] = "هذا المستخدم مدرس بالفعل";
                return RedirectToAction(nameof(AssignInstructor));
            }

            // إنشاء سجل المدرس
            var instructor = new EduPlatform.Core.Entities.Instructor
            {
                UserId = userId,
                Bio = bio,
                Status = InstructorStatus.Approved, // موافق عليه مباشرة
                RegisteredAt = DateTime.Now,
                ApprovedAt = DateTime.Now,
                ApprovedBy = _userManager.GetUserId(User)
            };

            _context.Instructors.Add(instructor);

            // إضافة دور المدرس
            await _userManager.AddToRoleAsync(user, "Instructor");

            await _context.SaveChangesAsync();

            TempData["Success"] = $"تم تعيين {user.FullName} كمدرس بنجاح";
            return RedirectToAction(nameof(Instructors));
        }

    }



}