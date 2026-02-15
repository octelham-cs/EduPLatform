using EduPlatform.Core.Entities;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Web.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // ========================================
        // GET: Register
        // ========================================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ========================================
        // POST: Register
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // التحقق من عدم وجود الإيميل مسبقاً
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم بالفعل");
                return View(model);
            }

            // التحقق من عدم وجود رقم الهاتف مسبقاً
            var existingPhone = await _userManager.Users
                .AnyAsync(u => u.PhoneNumber == model.PhoneNumber);
            if (existingPhone)
            {
                ModelState.AddModelError("PhoneNumber", "رقم الهاتف مستخدم بالفعل");
                return View(model);
            }

            // إنشاء المستخدم
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                GradeLevelId = model.GradeLevelId,
                BranchId = model.BranchId,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // إضافة دور الطالب
                await _userManager.AddToRoleAsync(user, "Student");

                // إنشاء سجل الطالب
                var student = new Student
                {
                    UserId = user.Id,
                    GradeLevelId = model.GradeLevelId,
                    BranchId = model.BranchId,
                    RegisteredAt = DateTime.Now
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                // تسجيل الدخول مباشرة
                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }

            // إضافة الأخطاء
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // ========================================
        // GET: Login
        // ========================================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // ========================================
        // POST: Login
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // البحث عن المستخدم بالإيميل أو رقم الهاتف
            var user = await _userManager.FindByEmailAsync(model.EmailOrPhone);

            if (user == null)
            {
                // محاولة البحث برقم الهاتف
                user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == model.EmailOrPhone);
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة السر غير صحيحة");
                return View(model);
            }

            // التحقق من كلمة السر
            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // تحديث آخر تسجيل دخول
                user.LastLogin = DateTime.Now;
                await _userManager.UpdateAsync(user);

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "تم قفل حسابك. حاول مرة أخرى بعد 5 دقائق");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة السر غير صحيحة");
            return View(model);
        }

        // ========================================
        // POST: Logout
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}