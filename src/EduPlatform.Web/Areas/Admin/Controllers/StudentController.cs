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

        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            int pageSize = 10;
            var query = _context.Students
                .Include(s => s.User)
                .Include(s => s.GradeLevel)
                .Include(s => s.Branch)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(s =>
                    s.User.FullName.Contains(searchTerm) ||
                    s.User.Email.Contains(searchTerm) ||
                    s.User.PhoneNumber.Contains(searchTerm));

            query = query.OrderByDescending(s => s.RegisteredAt);

            var totalItems = await query.CountAsync();
            var students = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new List<StudentListViewModel>();
            foreach (var student in students)
            {
                var count = await _context.Enrollments.CountAsync(e => e.StudentId == student.Id);
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
                    EnrollmentsCount = count
                });
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchTerm = searchTerm;

            return View(viewModel);
        }

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

            if (student == null) return NotFound();

            var progress = new Dictionary<string, int>();
            foreach (var enrollment in student.Enrollments.Where(e => e.Status == EnrollmentStatus.Active))
            {
                var total = await _context.Videos.CountAsync(v => v.Chapter.CourseId == enrollment.CourseId);
                var watched = await _context.VideoProgresses.CountAsync(vp =>
                    vp.StudentId == student.Id &&
                    vp.Video.Chapter.CourseId == enrollment.CourseId &&
                    vp.IsCompleted);
                progress[enrollment.Course.Title] = total > 0 ? (watched * 100) / total : 0;
            }

            var vm = new StudentDetailsViewModel
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
                    CourseName = e.Course?.Title ?? "-",
                    InstructorName = e.Course?.Instructor?.User?.FullName ?? "-",
                    EnrolledAt = e.EnrolledAt,
                    ExpiresAt = e.ExpiresAt,
                    Status = e.Status.ToString(),
                    Progress = progress.ContainsKey(e.Course?.Title ?? "") ? progress[e.Course.Title] : 0
                }).ToList(),
                Progress = progress
            };

            return View(vm);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null) return NotFound();

            return View(new StudentEditViewModel
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
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentEditViewModel vm)
        {
            if (id != vm.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.GradeLevels = await _context.GradeLevels.ToListAsync();
                vm.Branches = await _context.Branches.ToListAsync();
                return View(vm);
            }

            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound();

            try
            {
                student.User.FullName = vm.FullName;
                student.User.Email = vm.Email;
                student.User.UserName = vm.Email;
                student.User.NormalizedEmail = vm.Email.ToUpper();
                student.User.NormalizedUserName = vm.Email.ToUpper();
                student.User.PhoneNumber = vm.PhoneNumber;
                student.User.IsActive = vm.IsActive;
                student.GradeLevelId = vm.GradeLevelId ?? student.GradeLevelId;
                student.BranchId = vm.BranchId;

                await _context.SaveChangesAsync();
                TempData["Success"] = "تم تحديث بيانات الطالب بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث الطالب");
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث البيانات");
                vm.GradeLevels = await _context.GradeLevels.ToListAsync();
                vm.Branches = await _context.Branches.ToListAsync();
                return View(vm);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound();

            student.User.IsActive = !student.User.IsActive;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = student.User.IsActive,
                message = student.User.IsActive ? "تم تفعيل الحساب" : "تم تعطيل الحساب"
            });
        }

        public async Task<IActionResult> Enrollments(int id)
        {
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound();

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.User)
                .Include(e => e.EnrollmentCode)
                .Where(e => e.StudentId == id)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            ViewBag.StudentName = student.User?.FullName ?? "طالب";
            ViewBag.StudentId = id;
            return View(enrollments);
        }
    }
}