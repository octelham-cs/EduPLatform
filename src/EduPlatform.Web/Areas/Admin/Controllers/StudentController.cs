using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<StudentController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /Admin/Student
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            int pageSize = 10;

            // بنجيب كل الطلاب من جدول Students مباشرة
            var studentsQuery = _context.Students
                .Include(s => s.User)
                .Include(s => s.GradeLevel)
                .Include(s => s.Branch)
                .AsQueryable();

            // بحث
            if (!string.IsNullOrEmpty(searchTerm))
            {
                studentsQuery = studentsQuery.Where(s =>
                    s.User.FullName.Contains(searchTerm) ||
                    s.User.Email.Contains(searchTerm) ||
                    s.User.PhoneNumber.Contains(searchTerm));
            }

            // ترتيب حسب تاريخ التسجيل
            studentsQuery = studentsQuery.OrderByDescending(s => s.RegisteredAt);

            // Pagination
            var totalItems = await studentsQuery.CountAsync();
            var students = await studentsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // تحويل لـ ViewModel
            var viewModel = new List<StudentListViewModel>();
            foreach (var student in students)
            {
                var enrollmentsCount = await _context.Enrollments
                    .CountAsync(e => e.StudentId == student.Id);

                viewModel.Add(new StudentListViewModel
                {
                    Id = student.Id,
                    FullName = student.User.FullName,
                    Email = student.User.Email,
                    PhoneNumber = student.User.PhoneNumber,
                    GradeLevel = student.GradeLevel?.Name ?? "-",
                    Branch = student.Branch?.Name ?? "-",
                    RegisteredAt = student.RegisteredAt,
                    IsActive = student.User.IsActive,
                    EnrollmentsCount = enrollmentsCount
                });
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchTerm = searchTerm;

            return View(viewModel);
        }

        // GET: /Admin/Student/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.GradeLevel)
                .Include(s => s.Branch)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                        .ThenInclude(c => c.Instructor)
                            .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            // نجيب تقدم الطالب في كل مادة
            var progress = new Dictionary<string, int>();
            foreach (var enrollment in student.Enrollments.Where(e => e.Status == EnrollmentStatus.Active))
            {
                var totalVideos = await _context.Videos
                    .CountAsync(v => v.Chapter.CourseId == enrollment.CourseId);

                var watchedVideos = await _context.VideoProgresses
                    .CountAsync(vp => vp.StudentId == student.Id &&
                                      vp.Video.Chapter.CourseId == enrollment.CourseId &&
                                      vp.IsCompleted);

                var percentage = totalVideos > 0 ? (watchedVideos * 100) / totalVideos : 0;
                progress.Add(enrollment.Course.Title, percentage);
            }

            var viewModel = new StudentDetailsViewModel
            {
                Id = student.Id,
                FullName = student.User.FullName,
                Email = student.User.Email,
                PhoneNumber = student.User.PhoneNumber,
                GradeLevel = student.GradeLevel?.Name ?? "-",
                Branch = student.Branch?.Name ?? "-",
                RegisteredAt = student.RegisteredAt,
                IsActive = student.User.IsActive,
                LastLogin = student.User.LastLogin,
                Enrollments = student.Enrollments.Select(e => new StudentEnrollmentViewModel
                {
                    CourseName = e.Course.Title,
                    InstructorName = e.Course.Instructor?.User?.FullName ?? "-",
                    EnrolledAt = e.EnrolledAt,
                    ExpiresAt = e.ExpiresAt,
                    Status = e.Status.ToString(),
                    Progress = progress.ContainsKey(e.Course.Title) ? progress[e.Course.Title] : 0
                }).ToList(),
                Progress = progress
            };

            return View(viewModel);
        }

        // GET: /Admin/Student/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            var viewModel = new StudentEditViewModel
            {
                Id = student.Id,
                FullName = student.User.FullName,
                Email = student.User.Email,
                PhoneNumber = student.User.PhoneNumber,
                GradeLevelId = student.GradeLevelId,
                BranchId = student.BranchId,
                IsActive = student.User.IsActive,
                GradeLevels = await _context.GradeLevels.ToListAsync(),
                Branches = await _context.Branches.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: /Admin/Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                viewModel.GradeLevels = await _context.GradeLevels.ToListAsync();
                viewModel.Branches = await _context.Branches.ToListAsync();
                return View(viewModel);
            }

            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            try
            {
                // تعديل بيانات الطالب
                student.User.FullName = viewModel.FullName;
                student.User.Email = viewModel.Email;
                student.User.UserName = viewModel.Email; // مهم عشان Identity
                student.User.PhoneNumber = viewModel.PhoneNumber;
                student.User.IsActive = viewModel.IsActive;

                student.GradeLevelId = viewModel.GradeLevelId;
                student.BranchId = viewModel.BranchId;

                // لو غيرنا الإيميل، لازم نحدث الـ NormalizedEmail
                student.User.NormalizedEmail = viewModel.Email.ToUpper();
                student.User.NormalizedUserName = viewModel.Email.ToUpper();

                await _context.SaveChangesAsync();

                TempData["Success"] = "تم تحديث بيانات الطالب بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث الطالب");
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث البيانات");

                viewModel.GradeLevels = await _context.GradeLevels.ToListAsync();
                viewModel.Branches = await _context.Branches.ToListAsync();
                return View(viewModel);
            }
        }

        // POST: /Admin/Student/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }

            student.User.IsActive = !student.User.IsActive;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = student.User.IsActive,
                message = student.User.IsActive ? "تم تفعيل الحساب" : "تم تعطيل الحساب"
            });
        }

        // GET: /Admin/Student/Enrollments/5
        public async Task<IActionResult> Enrollments(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.User)
                .Include(e => e.EnrollmentCode)
                .Where(e => e.StudentId == id)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            ViewBag.StudentName = student.User?.FullName ?? "طالب";
            return View(enrollments);
        }
    }
}