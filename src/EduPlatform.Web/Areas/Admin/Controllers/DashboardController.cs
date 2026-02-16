using EduPlatform.Core.Entities;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Core.Enums;
using EduPlatform.Web.ViewModels.Admin;
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
            var totalInstructors = await _context.Instructors.CountAsync();
            var pendingInstructors = await _context.Instructors
                .CountAsync(i => i.Status == InstructorStatus.Pending);
            var approvedInstructors = await _context.Instructors
                .CountAsync(i => i.Status == InstructorStatus.Approved);
            var totalStudents = await _context.Students.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var totalVideos = await _context.Videos.CountAsync();
            var totalCodes = await _context.EnrollmentCodes.CountAsync();

            // قائمة المدرسين المعلقين
            var pendingInstructorsList = await _context.Instructors
                .Include(i => i.User)
                .Where(i => i.Status == InstructorStatus.Pending)
                .OrderByDescending(i => i.RegisteredAt)
                .Take(10)
                .Select(i => new PendingInstructorViewModel
                {
                    Id = i.Id,
                    Name = i.User != null ? i.User.FullName : "غير معروف",
                    Email = i.User != null ? i.User.Email : "",
                    RegisteredAt = i.RegisteredAt
                })
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalInstructors = totalInstructors,
                PendingInstructors = pendingInstructors,
                ApprovedInstructors = approvedInstructors,
                TotalStudents = totalStudents,
                TotalCourses = totalCourses,
                TotalVideos = totalVideos,
                TotalCodes = totalCodes,
                PendingInstructorsList = pendingInstructorsList
            };

            return View(viewModel);
        }

        // ========================================
        // GET: Admin/Instructors
        // ========================================
        public async Task<IActionResult> Instructors()
        {
            var instructors = await _context.Instructors
                .Include(i => i.User)
                .Include(i => i.Subject)
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
                .OrderBy(s => s.GradeLevelId)
                .ToListAsync();

            // جلب أسماء المستويات الدراسية
            var gradeLevels = await _context.GradeLevels
                .ToDictionaryAsync(g => g.Id, g => g.Name);

            ViewBag.GradeLevels = gradeLevels;

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

            var adminUserIds = (await _userManager.GetUsersInRoleAsync("Admin"))
                .Select(u => u.Id)
                .ToList();

            var users = await _userManager.Users
                .Where(u => !instructorUserIds.Contains(u.Id) && !adminUserIds.Contains(u.Id))
                .OrderBy(u => u.FullName)
                .ToListAsync();

            // جلب المواد
            var subjects = await _context.Subjects
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.Subjects = subjects;
            ViewBag.GeneratedPassword = GenerateRandomPassword();

            return View(users);
        }

        // ========================================
        // POST: Admin/AssignInstructor (لتعيين مستخدم موجود)
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignInstructor(AssignInstructorViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(model.UserId))
            {
                TempData["Error"] = "يرجى اختيار مستخدم ومادة";
                return RedirectToAction(nameof(AssignInstructor));
            }

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                TempData["Error"] = "المستخدم غير موجود";
                return RedirectToAction(nameof(AssignInstructor));
            }

            // التحقق إن المستخدم مش مدرس بالفعل
            var existingInstructor = await _context.Instructors
                .AnyAsync(i => i.UserId == model.UserId);

            if (existingInstructor)
            {
                TempData["Error"] = "هذا المستخدم مدرس بالفعل";
                return RedirectToAction(nameof(AssignInstructor));
            }

            // إنشاء سجل المدرس
            var instructor = new EduPlatform.Core.Entities.Instructor
            {
                UserId = model.UserId,
                Bio = model.Bio,
                SubjectId = model.SubjectId,
                Status = InstructorStatus.Approved,
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

        // ========================================
        // POST: Admin/CreateAndAssignInstructor
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAndAssignInstructor(CreateInstructorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "يرجى إدخال البيانات بشكل صحيح";
                TempData["ActiveTab"] = "new-user-tab";

                // تخزين البيانات في TempData بشكل منفصل
                TempData["FormFullName"] = model.FullName;
                TempData["FormEmail"] = model.Email;
                TempData["FormPhoneNumber"] = model.PhoneNumber;
                TempData["FormBio"] = model.Bio;

                return RedirectToAction(nameof(AssignInstructor));
            }

            // التحقق من عدم وجود الإيميل مسبقاً
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم بالفعل");
                TempData["Error"] = "البريد الإلكتروني مستخدم بالفعل";
                TempData["ActiveTab"] = "new-user-tab";
                TempData["FormFullName"] = model.FullName;
                TempData["FormEmail"] = model.Email;
                TempData["FormPhoneNumber"] = model.PhoneNumber;
                TempData["FormBio"] = model.Bio;
                return RedirectToAction(nameof(AssignInstructor));
            }

            // التحقق من عدم وجود رقم الهاتف مسبقاً
            var existingPhone = await _userManager.Users
                .AnyAsync(u => u.PhoneNumber == model.PhoneNumber);
            if (existingPhone)
            {
                ModelState.AddModelError("PhoneNumber", "رقم الهاتف مستخدم بالفعل");
                TempData["Error"] = "رقم الهاتف مستخدم بالفعل";
                TempData["ActiveTab"] = "new-user-tab";
                TempData["FormFullName"] = model.FullName;
                TempData["FormEmail"] = model.Email;
                TempData["FormPhoneNumber"] = model.PhoneNumber;
                TempData["FormBio"] = model.Bio;
                return RedirectToAction(nameof(AssignInstructor));
            }

            // إنشاء كلمة مرور عشوائية
            var password = GenerateRandomPassword();

            // إنشاء المستخدم الجديد
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // إضافة دور المدرس
                await _userManager.AddToRoleAsync(user, "Instructor");

                // إنشاء سجل المدرس
                var instructor = new EduPlatform.Core.Entities.Instructor
                {
                    UserId = user.Id,
                    Bio = model.Bio,
                    SubjectId = model.SubjectId,
                    Status = InstructorStatus.Approved,
                    RegisteredAt = DateTime.Now,
                    ApprovedAt = DateTime.Now,
                    ApprovedBy = _userManager.GetUserId(User)
                };

                _context.Instructors.Add(instructor);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"✅ تم إنشاء وتعيين {user.FullName} كمدرس بنجاح.\nكلمة المرور: {password}";
                return RedirectToAction(nameof(Instructors));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            TempData["Error"] = "حدث خطأ أثناء إنشاء المستخدم";
            TempData["ActiveTab"] = "new-user-tab";
            TempData["FormFullName"] = model.FullName;
            TempData["FormEmail"] = model.Email;
            TempData["FormPhoneNumber"] = model.PhoneNumber;
            TempData["FormBio"] = model.Bio;
            return RedirectToAction(nameof(AssignInstructor));
        }

        // ========================================
        // دالة مساعدة لتوليد كلمة مرور عشوائية
        // ========================================
        private string GenerateRandomPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return password + "A1!";
        }
    }
}