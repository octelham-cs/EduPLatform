// src/EduPlatform.Web/Areas/Instructor/Controllers/CodeController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Web.ViewModels.Instructor;
using Microsoft.AspNetCore.Identity;

namespace EduPlatform.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class CodeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CodeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Instructor/Code
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null)
            {
                TempData["Error"] = "لم يتم العثور على حساب المدرس";
                return RedirectToAction("Index", "Dashboard");
            }

            var codes = await _context.EnrollmentCodes
                .Include(e => e.Course)
                .Include(e => e.AcademicTerm)
                .Include(e => e.Student)
                    .ThenInclude(s => s!.User)
                .Where(e => e.InstructorId == instructor.Id)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var viewModel = codes.Select(c => new GeneratedCodeViewModel
            {
                Id = c.Id,
                Code = c.Code,
                CourseName = c.Course?.Title ?? "غير محدد",
                Price = c.Price,
                Status = c.Status switch
                {
                    CodeStatus.Available => "متاح",
                    CodeStatus.Used => "مستخدم",
                    CodeStatus.Expired => "منتهي",
                    _ => "غير معروف"
                },
                CreatedAt = c.CreatedAt,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                StudentName = c.Student?.User?.FullName,
                UsedAt = c.UsedAt
            }).ToList();

            return View(viewModel);
        }

        // GET: /Instructor/Code/Generate
        public async Task<IActionResult> Generate()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null)
            {
                TempData["Error"] = "لم يتم العثور على حساب المدرس";
                return RedirectToAction("Index", "Dashboard");
            }

            // تحميل الكورسات
            var courses = await _context.Courses
                .Where(c => c.InstructorId == instructor.Id && c.IsActive)
                .Select(c => new { c.Id, c.Title, c.Price })
                .ToListAsync();

            ViewBag.Courses = new SelectList(courses, "Id", "Title");
            ViewBag.CoursePrices = courses.ToDictionary(c => c.Id, c => c.Price);

            // تحميل الأترمة الدراسية - كـ List عادية وليس SelectList
            var terms = await _context.AcademicTerms
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.StartDate)
                .Select(t => new AcademicTermViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate
                })
                .ToListAsync();

            ViewBag.AcademicTerms = terms; // ← الآن هي List وليس SelectList

            // تعيين التواريخ الافتراضية
            var today = DateTime.Today;
            var nextMonth = today.AddMonths(1);

            var model = new GenerateCodeViewModel
            {
                StartDate = today,
                EndDate = nextMonth
            };

            return View(model);
        }

        // POST: /Instructor/Code/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(GenerateCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null)
            {
                TempData["Error"] = "لم يتم العثور على حساب المدرس";
                return RedirectToAction("Index", "Dashboard");
            }

            // التحقق من التواريخ
            if (model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("", "تاريخ النهاية يجب أن يكون بعد تاريخ البداية");
                await LoadDropdowns();
                return View(model);
            }

            var codes = new List<EnrollmentCode>();

            for (int i = 0; i < model.Quantity; i++)
            {
                var code = new EnrollmentCode
                {
                    Code = await GenerateUniqueCode(),
                    InstructorId = instructor.Id,
                    CourseId = model.CourseId,
                    AcademicTermId = model.AcademicTermId,
                    Price = model.Price,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    CreatedAt = DateTime.Now,
                    Status = CodeStatus.Available
                };

                // تطبيق الخصم
                if (model.DiscountPercentage.HasValue && model.DiscountPercentage > 0)
                {
                    var discountAmount = model.Price * (model.DiscountPercentage.Value / 100m);
                    code.Price = model.Price - discountAmount;
                }

                codes.Add(code);
            }

            _context.EnrollmentCodes.AddRange(codes);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"تم توليد {model.Quantity} كود بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Instructor/Code/ExportCsv
        public async Task<IActionResult> ExportCsv()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null) return NotFound();

            var codes = await _context.EnrollmentCodes
                .Include(e => e.Course)
                .Where(e => e.InstructorId == instructor.Id)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("الكود,الكورس,السعر,الحالة,تاريخ الإنشاء,تاريخ البداية,تاريخ النهاية,الطالب");

            foreach (var code in codes)
            {
                var status = code.Status switch
                {
                    CodeStatus.Available => "متاح",
                    CodeStatus.Used => "مستخدم",
                    CodeStatus.Expired => "منتهي",
                    _ => "غير معروف"
                };

                csv.AppendLine($"\"{code.Code}\",\"{code.Course?.Title ?? "غير محدد"}\",{code.Price},\"{status}\",{code.CreatedAt:yyyy-MM-dd},{code.StartDate:yyyy-MM-dd},{code.EndDate:yyyy-MM-dd},\"{code.Student?.User?.FullName ?? ""}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"codes_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        // GET: /Instructor/Code/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var code = await _context.EnrollmentCodes
                .Include(e => e.Course)
                .Include(e => e.AcademicTerm)
                .Include(e => e.Student)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(e => e.Id == id && e.InstructorId == instructor!.Id);

            if (code == null) return NotFound();

            var viewModel = new GeneratedCodeViewModel
            {
                Id = code.Id,
                Code = code.Code,
                CourseName = code.Course?.Title ?? "غير محدد",
                Price = code.Price,
                Status = code.Status switch
                {
                    CodeStatus.Available => "متاح",
                    CodeStatus.Used => "مستخدم",
                    CodeStatus.Expired => "منتهي",
                    _ => "غير معروف"
                },
                CreatedAt = code.CreatedAt,
                StartDate = code.StartDate,
                EndDate = code.EndDate,
                StudentName = code.Student?.User?.FullName,
                UsedAt = code.UsedAt
            };

            return View(viewModel);
        }

        private async Task LoadDropdowns()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var courses = await _context.Courses
                .Where(c => c.InstructorId == instructor!.Id && c.IsActive)
                .Select(c => new { c.Id, c.Title, c.Price })
                .ToListAsync();

            ViewBag.Courses = new SelectList(courses, "Id", "Title");

            // هنا أيضاً نستخدم List وليس SelectList
            var terms = await _context.AcademicTerms
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.StartDate)
                .Select(t => new AcademicTermViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate
                })
                .ToListAsync();

            ViewBag.AcademicTerms = terms;
        }

        private async Task<string> GenerateUniqueCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;
            bool exists;

            do
            {
                code = "EDU-" + new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                exists = await _context.EnrollmentCodes.AnyAsync(e => e.Code == code);
            } while (exists);

            return code;
        }
    }
}

// أضف هذا الكلاس خارج الـ Controller (في نفس الملف)
public class AcademicTermViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}