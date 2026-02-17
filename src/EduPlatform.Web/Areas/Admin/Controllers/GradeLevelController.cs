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
    public class GradeLevelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GradeLevelController> _logger;

        public GradeLevelController(ApplicationDbContext context, ILogger<GradeLevelController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/GradeLevel
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            int pageSize = 10;

            var query = _context.GradeLevels.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(g =>
                    g.Name.Contains(searchTerm) ||
                    g.NameEn.Contains(searchTerm));
            }

            query = query.OrderBy(g => g.Order);

            var totalItems = await query.CountAsync();
            var gradeLevels = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new List<GradeLevelListViewModel>();
            foreach (var g in gradeLevels)
            {
                var subjectsCount = await _context.Subjects
                    .CountAsync(s => s.GradeLevelId == g.Id);

                var studentsCount = await _context.Students
                    .CountAsync(s => s.GradeLevelId == g.Id);

                viewModel.Add(new GradeLevelListViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    NameEn = g.NameEn,
                    Order = g.Order,
                    IsActive = g.IsActive,
                    SubjectsCount = subjectsCount,
                    StudentsCount = studentsCount
                });
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchTerm = searchTerm;

            return View(viewModel);
        }

        // GET: /Admin/GradeLevel/Create
        public IActionResult Create()
        {
            return View(new GradeLevelCreateViewModel());
        }

        // POST: /Admin/GradeLevel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GradeLevelCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                // نتأكد إن مفيش نفس الترتيب
                var existingOrder = await _context.GradeLevels
                    .AnyAsync(g => g.Order == viewModel.Order);

                if (existingOrder)
                {
                    ModelState.AddModelError("Order", "هذا الترتيب موجود بالفعل");
                    return View(viewModel);
                }

                var gradeLevel = new GradeLevel
                {
                    Name = viewModel.Name,
                    NameEn = viewModel.NameEn,
                    Order = viewModel.Order,
                    IsActive = viewModel.IsActive
                };

                _context.GradeLevels.Add(gradeLevel);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إضافة المستوى بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة مستوى");
                ModelState.AddModelError("", "حدث خطأ أثناء إضافة المستوى");
                return View(viewModel);
            }
        }

        // GET: /Admin/GradeLevel/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var gradeLevel = await _context.GradeLevels.FindAsync(id);
            if (gradeLevel == null)
            {
                return NotFound();
            }

            var viewModel = new GradeLevelEditViewModel
            {
                Id = gradeLevel.Id,
                Name = gradeLevel.Name,
                NameEn = gradeLevel.NameEn,
                Order = gradeLevel.Order,
                IsActive = gradeLevel.IsActive
            };

            return View(viewModel);
        }

        // POST: /Admin/GradeLevel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GradeLevelEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                // نتأكد إن مفيش نفس الترتيب لمستوى تاني
                var existingOrder = await _context.GradeLevels
                    .AnyAsync(g => g.Order == viewModel.Order && g.Id != id);

                if (existingOrder)
                {
                    ModelState.AddModelError("Order", "هذا الترتيب موجود بالفعل");
                    return View(viewModel);
                }

                var gradeLevel = await _context.GradeLevels.FindAsync(id);
                if (gradeLevel == null)
                {
                    return NotFound();
                }

                gradeLevel.Name = viewModel.Name;
                gradeLevel.NameEn = viewModel.NameEn;
                gradeLevel.Order = viewModel.Order;
                gradeLevel.IsActive = viewModel.IsActive;

                await _context.SaveChangesAsync();

                TempData["Success"] = "تم تحديث المستوى بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث المستوى");
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث المستوى");
                return View(viewModel);
            }
        }

        // GET: /Admin/GradeLevel/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var gradeLevel = await _context.GradeLevels
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gradeLevel == null)
            {
                return NotFound();
            }

            // المواد في هذا المستوى
            var subjects = await _context.Subjects
                .Where(s => s.GradeLevelId == id)
                .ToListAsync();

            var subjectViewModels = new List<GradeLevelSubjectViewModel>();
            foreach (var s in subjects)
            {
                string branchName = "عام";
                if (s.BranchId.HasValue)
                {
                    var branch = await _context.Branches.FindAsync(s.BranchId.Value);
                    branchName = branch != null ? branch.Name : "عام";
                }

                var instructorsCount = await _context.Courses
                    .Where(c => c.SubjectId == s.Id)
                    .Select(c => c.InstructorId)
                    .Distinct()
                    .CountAsync();

                subjectViewModels.Add(new GradeLevelSubjectViewModel
                {
                    SubjectId = s.Id,
                    SubjectName = s.Name,
                    Branch = branchName,
                    InstructorsCount = instructorsCount
                });
            }

            // الطلاب في هذا المستوى
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Branch)
                .Where(s => s.GradeLevelId == id)
                .Take(20) // آخر 20 طالب
                .OrderByDescending(s => s.RegisteredAt)
                .ToListAsync();

            var studentViewModels = students.Select(s => new GradeLevelStudentViewModel
            {
                StudentId = s.Id,
                StudentName = s.User != null ? s.User.FullName : "-",
                Email = s.User != null ? s.User.Email : "-",
                Branch = s.Branch != null ? s.Branch.Name : "-",
                RegisteredAt = s.RegisteredAt
            }).ToList();

            var viewModel = new GradeLevelDetailsViewModel
            {
                Id = gradeLevel.Id,
                Name = gradeLevel.Name,
                NameEn = gradeLevel.NameEn,
                Order = gradeLevel.Order,
                IsActive = gradeLevel.IsActive,
                Subjects = subjectViewModels,
                Students = studentViewModels
            };

            return View(viewModel);
        }

        // POST: /Admin/GradeLevel/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var gradeLevel = await _context.GradeLevels.FindAsync(id);
                if (gradeLevel == null)
                {
                    return NotFound();
                }

                // نتأكد إن مفيش مواد أو طلاب مرتبطة بالمستوى
                var hasSubjects = await _context.Subjects.AnyAsync(s => s.GradeLevelId == id);
                var hasStudents = await _context.Students.AnyAsync(s => s.GradeLevelId == id);

                if (hasSubjects || hasStudents)
                {
                    return Json(new
                    {
                        success = false,
                        message = "لا يمكن حذف مستوى له مواد أو طلاب مرتبطة به"
                    });
                }

                _context.GradeLevels.Remove(gradeLevel);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم حذف المستوى بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المستوى");
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف" });
            }
        }

        // POST: /Admin/GradeLevel/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var gradeLevel = await _context.GradeLevels.FindAsync(id);
            if (gradeLevel == null)
            {
                return NotFound();
            }

            gradeLevel.IsActive = !gradeLevel.IsActive;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = gradeLevel.IsActive,
                message = gradeLevel.IsActive ? "تم تفعيل المستوى" : "تم تعطيل المستوى"
            });
        }
    }
}