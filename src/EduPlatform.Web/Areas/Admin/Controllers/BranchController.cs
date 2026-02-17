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
    public class BranchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BranchController> _logger;

        public BranchController(ApplicationDbContext context, ILogger<BranchController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: /Admin/Branch
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            int pageSize = 10;

            var query = _context.Branches.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(b =>
                    b.Name.Contains(searchTerm) ||
                    b.NameEn.Contains(searchTerm));
            }

            query = query.OrderBy(b => b.Name);

            var totalItems = await query.CountAsync();
            var branches = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new List<BranchListViewModel>();
            foreach (var b in branches)
            {
                var subjectsCount = await _context.Subjects
                    .CountAsync(s => s.BranchId == b.Id);

                var studentsCount = await _context.Students
                    .CountAsync(s => s.BranchId == b.Id);

                viewModel.Add(new BranchListViewModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    NameEn = b.NameEn,
                    IsActive = b.IsActive,
                    SubjectsCount = subjectsCount,
                    StudentsCount = studentsCount
                });
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SearchTerm = searchTerm;

            return View(viewModel);
        }

        // GET: /Admin/Branch/Create
        public IActionResult Create()
        {
            return View(new BranchCreateViewModel());
        }

        // POST: /Admin/Branch/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                // نتأكد إن مفيش نفس الاسم
                var existingName = await _context.Branches
                    .AnyAsync(b => b.Name == viewModel.Name);

                if (existingName)
                {
                    ModelState.AddModelError("Name", "هذا الاسم موجود بالفعل");
                    return View(viewModel);
                }

                var branch = new Branch
                {
                    Name = viewModel.Name,
                    NameEn = viewModel.NameEn,
                    IsActive = viewModel.IsActive
                };

                _context.Branches.Add(branch);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تم إضافة الشعبة بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة شعبة");
                ModelState.AddModelError("", "حدث خطأ أثناء إضافة الشعبة");
                return View(viewModel);
            }
        }

        // GET: /Admin/Branch/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound();
            }

            var viewModel = new BranchEditViewModel
            {
                Id = branch.Id,
                Name = branch.Name,
                NameEn = branch.NameEn,
                IsActive = branch.IsActive
            };

            return View(viewModel);
        }

        // POST: /Admin/Branch/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BranchEditViewModel viewModel)
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
                // نتأكد إن مفيش نفس الاسم لشعبة تانية
                var existingName = await _context.Branches
                    .AnyAsync(b => b.Name == viewModel.Name && b.Id != id);

                if (existingName)
                {
                    ModelState.AddModelError("Name", "هذا الاسم موجود بالفعل");
                    return View(viewModel);
                }

                var branch = await _context.Branches.FindAsync(id);
                if (branch == null)
                {
                    return NotFound();
                }

                branch.Name = viewModel.Name;
                branch.NameEn = viewModel.NameEn;
                branch.IsActive = viewModel.IsActive;

                await _context.SaveChangesAsync();

                TempData["Success"] = "تم تحديث الشعبة بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث الشعبة");
                ModelState.AddModelError("", "حدث خطأ أثناء تحديث الشعبة");
                return View(viewModel);
            }
        }

        // GET: /Admin/Branch/Details/5
        // GET: /Admin/Branch/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null)
            {
                return NotFound();
            }

            // المواد في هذه الشعبة
            var subjects = await _context.Subjects
                .Where(s => s.BranchId == id)
                .ToListAsync();

            var subjectViewModels = new List<BranchSubjectViewModel>();
            foreach (var s in subjects)
            {
                // جلب اسم المستوى من جدول GradeLevel
                var gradeLevel = await _context.GradeLevels.FindAsync(s.GradeLevelId);
                string gradeLevelName = gradeLevel != null ? gradeLevel.Name : "-";

                var instructorsCount = await _context.Courses
                    .Where(c => c.SubjectId == s.Id)
                    .Select(c => c.InstructorId)
                    .Distinct()
                    .CountAsync();

                subjectViewModels.Add(new BranchSubjectViewModel
                {
                    SubjectId = s.Id,
                    SubjectName = s.Name,
                    GradeLevel = gradeLevelName,
                    InstructorsCount = instructorsCount
                });
            }

            // الطلاب في هذه الشعبة
            var students = await _context.Students
                .Include(s => s.User)
                .Include(s => s.GradeLevel)
                .Where(s => s.BranchId == id)
                .Take(20)
                .OrderByDescending(s => s.RegisteredAt)
                .ToListAsync();

            var studentViewModels = students.Select(s => new BranchStudentViewModel
            {
                StudentId = s.Id,
                StudentName = s.User != null ? s.User.FullName : "-",
                Email = s.User != null ? s.User.Email : "-",
                GradeLevel = s.GradeLevel != null ? s.GradeLevel.Name : "-",
                RegisteredAt = s.RegisteredAt
            }).ToList();

            var viewModel = new BranchDetailsViewModel
            {
                Id = branch.Id,
                Name = branch.Name,
                NameEn = branch.NameEn,
                IsActive = branch.IsActive,
                Subjects = subjectViewModels,
                Students = studentViewModels
            };

            return View(viewModel);
        }

        // POST: /Admin/Branch/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var branch = await _context.Branches.FindAsync(id);
                if (branch == null)
                {
                    return NotFound();
                }

                // نتأكد إن مفيش مواد أو طلاب مرتبطة بالشعبة
                var hasSubjects = await _context.Subjects.AnyAsync(s => s.BranchId == id);
                var hasStudents = await _context.Students.AnyAsync(s => s.BranchId == id);

                if (hasSubjects || hasStudents)
                {
                    return Json(new
                    {
                        success = false,
                        message = "لا يمكن حذف شعبة لها مواد أو طلاب مرتبطة بها"
                    });
                }

                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم حذف الشعبة بنجاح" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف الشعبة");
                return Json(new { success = false, message = "حدث خطأ أثناء الحذف" });
            }
        }

        // POST: /Admin/Branch/ToggleStatus/5
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound();
            }

            branch.IsActive = !branch.IsActive;
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                isActive = branch.IsActive,
                message = branch.IsActive ? "تم تفعيل الشعبة" : "تم تعطيل الشعبة"
            });
        }
    }
}