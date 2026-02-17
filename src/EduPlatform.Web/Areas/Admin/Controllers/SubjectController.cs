using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduPlatform.Core.Entities;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Web.ViewModels.Admin;

namespace EduPlatform.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SubjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubjectController> _logger;

        public SubjectController(ApplicationDbContext context, ILogger<SubjectController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Subject
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            int pageSize = 10;

            var subjectsQuery = _context.Subjects.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                subjectsQuery = subjectsQuery.Where(s =>
                    s.Name.Contains(searchTerm) ||
                    s.NameEn.Contains(searchTerm));
            }

            subjectsQuery = subjectsQuery.OrderBy(s => s.Id);

            var totalItems = await subjectsQuery.CountAsync();
            var subjects = await subjectsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new List<SubjectListViewModel>();
            foreach (var s in subjects)
            {
                var gradeLevel = await _context.GradeLevels.FindAsync(s.GradeLevelId);
                string gradeLevelName = gradeLevel != null ? gradeLevel.Name : "-";

                string branchName = "عام";
                if (s.BranchId.HasValue)
                {
                    var branch = await _context.Branches.FindAsync(s.BranchId.Value);
                    branchName = branch != null ? branch.Name : "عام";
                }

                var courses = await _context.Courses
                    .Where(c => c.SubjectId == s.Id)
                    .ToListAsync();

                viewModel.Add(new SubjectListViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    NameEn = s.NameEn,
                    GradeLevel = gradeLevelName,
                    Branch = branchName,
                    IsActive = s.IsActive,
                    InstructorsCount = courses.Select(c => c.InstructorId).Distinct().Count(),
                    CoursesCount = courses.Count
                });
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchTerm = searchTerm;

            return View(viewModel);
        }

        // GET: /Admin/Subject/Create
        public async Task<IActionResult> Create()
        {
            var viewModel = new SubjectCreateViewModel
            {
                GradeLevels = await _context.GradeLevels.ToListAsync(),
                Branches = await _context.Branches.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: /Admin/Subject/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubjectCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.GradeLevels = await _context.GradeLevels.ToListAsync();
                viewModel.Branches = await _context.Branches.ToListAsync();
                return View(viewModel);
            }

            try
            {
                var subject = new Subject
                {
                    Name = viewModel.Name,
                    NameEn = viewModel.NameEn,
                    GradeLevelId = viewModel.GradeLevelId,
                    BranchId = viewModel.BranchId,
                    IsActive = viewModel.IsActive
                };

                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إضافة المادة بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة مادة");
                ModelState.AddModelError("", "حدث خطأ أثناء إضافة المادة");

                viewModel.GradeLevels = await _context.GradeLevels.ToListAsync();
                viewModel.Branches = await _context.Branches.ToListAsync();
                return View(viewModel);
            }
        }

        // GET: /Admin/Subject/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            var viewModel = new SubjectEditViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                NameEn = subject.NameEn,
                GradeLevelId = subject.GradeLevelId,
                BranchId = subject.BranchId,
                IsActive = subject.IsActive,
                GradeLevels = await _context.GradeLevels.ToListAsync(),
                Branches = await _context.Branches.ToListAsync()
            };

            return View(viewModel);
        }

        // POST: /Admin/Subject/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubjectEditViewModel viewModel)
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

            try
            {
                var subject = await _context.Subjects.FindAsync(id);
                if (subject == null)
                {
                    return NotFound();
                }

                subject.Name = viewModel.Name;
                subject.NameEn = viewModel.NameEn;
                subject.GradeLevelId = viewModel.GradeLevelId;
                subject.BranchId = viewModel.BranchId;
                subject.IsActive = viewModel.IsActive;

                await _context.SaveChangesAsync();

                TempData["Success"] = "تم تحديث المادة بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث المادة");
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث المادة");

                viewModel.GradeLevels = await _context.GradeLevels.ToListAsync();
                viewModel.Branches = await _context.Branches.ToListAsync();
                return View(viewModel);
            }
        }

        // GET: /Admin/Subject/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            var gradeLevel = await _context.GradeLevels.FindAsync(subject.GradeLevelId);
            string gradeLevelName = gradeLevel != null ? gradeLevel.Name : "-";

            string branchName = "عام";
            if (subject.BranchId.HasValue)
            {
                var branch = await _context.Branches.FindAsync(subject.BranchId.Value);
                branchName = branch != null ? branch.Name : "عام";
            }

            var courses = await _context.Courses
                .Include(c => c.Instructor)
                    .ThenInclude(i => i.User)
                .Where(c => c.SubjectId == id)
                .ToListAsync();

            var instructors = courses
                .GroupBy(c => c.InstructorId)
                .Select(g => g.First())
                .Select(c => new SubjectInstructorViewModel
                {
                    InstructorId = c.Instructor.Id,
                    Name = c.Instructor.User != null ? c.Instructor.User.FullName : "-",
                    Email = c.Instructor.User != null ? c.Instructor.User.Email : "-",
                    StudentsCount = _context.Enrollments.Count(e => e.CourseId == c.Id)
                }).ToList();

            var courseViewModels = courses.Select(c => new SubjectCourseViewModel
            {
                CourseId = c.Id,
                Title = c.Title,
                InstructorName = c.Instructor.User != null ? c.Instructor.User.FullName : "-",
                Price = c.Price,
                StudentsCount = _context.Enrollments.Count(e => e.CourseId == c.Id)
            }).ToList();

            var viewModel = new SubjectDetailsViewModel
            {
                Id = subject.Id,
                Name = subject.Name,
                NameEn = subject.NameEn,
                GradeLevel = gradeLevelName,
                Branch = branchName,
                IsActive = subject.IsActive,
                Instructors = instructors,
                Courses = courseViewModels
            };

            return View(viewModel);
        }

        // POST: /Admin/Subject/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var subject = await _context.Subjects.FindAsync(id);
                if (subject == null)
                {
                    return NotFound();
                }

                var hasCourses = await _context.Courses.AnyAsync(c => c.SubjectId == id);
                if (hasCourses)
                {
                    return Json(new { success = false, message = "لا يمكن حذف مادة لها كورسات مرتبطة" });
                }

                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم حذف المادة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المادة");
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف" });
            }
        }

        // POST: /Admin/Subject/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject == null)
            {
                return NotFound();
            }

            subject.IsActive = !subject.IsActive;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = subject.IsActive,
                message = subject.IsActive ? "تم تفعيل المادة" : "تم تعطيل المادة"
            });
        }
    }
}