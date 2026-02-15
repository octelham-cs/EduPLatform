using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Web.ViewModels.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class CodeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CodeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ========================================
        // GET: Instructor/Code
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

            var codes = await _context.EnrollmentCodes
                .Include(c => c.Course)
                .Include(c => c.AcademicTerm)
                .Include(c => c.UsedByStudent)
                    .ThenInclude(s => s.User)
                .Where(c => c.InstructorId == instructor.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(codes);
        }

        // ========================================
        // GET: Instructor/Code/Generate
        // ========================================
        [HttpGet]
        public async Task<IActionResult> Generate()
        {
            var user = await _userManager.GetUserAsync(User);
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null)
            {
                return NotFound();
            }

            // جلب كورسات المدرس
            ViewBag.Courses = await _context.Courses
                .Include(c => c.Subject)
                .Where(c => c.InstructorId == instructor.Id && c.IsActive)
                .ToListAsync();

            // جلب الأترمة الدراسية
            ViewBag.AcademicTerms = await _context.AcademicTerms
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.Year)
                .ToListAsync();

            return View();
        }

        // ========================================
        // POST: Instructor/Code/Generate
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(GenerateCodeViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Courses = await _context.Courses
                    .Include(c => c.Subject)
                    .Where(c => c.InstructorId == instructor.Id && c.IsActive)
                    .ToListAsync();

                ViewBag.AcademicTerms = await _context.AcademicTerms
                    .Where(t => t.IsActive)
                    .ToListAsync();

                return View(model);
            }

            // جلب الترم الدراسي
            var term = await _context.AcademicTerms.FindAsync(model.AcademicTermId);
            if (term == null)
            {
                ModelState.AddModelError("AcademicTermId", "الترم الدراسي غير موجود");
                return View(model);
            }

            // حساب السعر بعد الخصم
            var finalPrice = model.Price;
            if (model.DiscountPercentage.HasValue && model.DiscountPercentage > 0)
            {
                finalPrice = model.Price - (model.Price * model.DiscountPercentage.Value / 100);
            }

            // توليد الأكواد
            var codes = new List<EnrollmentCode>();
            for (int i = 0; i < model.Quantity; i++)
            {
                var code = new EnrollmentCode
                {
                    Code = GenerateRandomCode(),
                    InstructorId = instructor.Id,
                    CourseId = model.CourseId,
                    AcademicTermId = model.AcademicTermId,
                    Price = finalPrice,
                    DiscountPercentage = model.DiscountPercentage,
                    Status = CodeStatus.Available,
                    CreatedAt = DateTime.Now,
                    StartDate = term.StartDate,
                    EndDate = term.EndDate,
                    Notes = model.Notes
                };
                codes.Add(code);
            }

            _context.EnrollmentCodes.AddRange(codes);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"تم توليد {model.Quantity} كود بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // ========================================
        // GET: Instructor/Code/Export
        // ========================================
        public async Task<IActionResult> Export(int? courseId, string? status)
        {
            var user = await _userManager.GetUserAsync(User);
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null)
            {
                return NotFound();
            }

            var query = _context.EnrollmentCodes
                .Include(c => c.Course)
                .Include(c => c.AcademicTerm)
                .Where(c => c.InstructorId == instructor.Id);

            if (courseId.HasValue)
            {
                query = query.Where(c => c.CourseId == courseId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<CodeStatus>(status, out var codeStatus))
                {
                    query = query.Where(c => c.Status == codeStatus);
                }
            }

            var codes = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

            // إنشاء CSV بسيط
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("الكود,الكورس,السعر,الحالة,تاريخ الإنشاء,تاريخ الانتهاء");

            foreach (var code in codes)
            {
                var statusText = code.Status switch
                {
                    CodeStatus.Available => "متاح",
                    CodeStatus.Used => "مستخدم",
                    CodeStatus.Expired => "منتهي",
                    _ => "غير معروف"
                };

                csv.AppendLine($"{code.Code},{code.Course.Title},{code.Price},{statusText},{code.CreatedAt:yyyy/MM/dd},{code.EndDate:yyyy/MM/dd}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "codes.csv");
        }

        // ========================================
        // Helper: توليد كود عشوائي
        // ========================================
        private string GenerateRandomCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            // تنسيق: EDU-XXXX-XXXX
            var part1 = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            var part2 = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return $"EDU-{part1}-{part2}";
        }
    }
}