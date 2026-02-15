using EduPlatform.Core.Enums;
using EduPlatform.Core.Interfaces;
using EduPlatform.Web.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EduPlatform.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize] // لازم يكون مسجل دخول
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;

        public EnrollmentController(IEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        [HttpGet]
        public IActionResult Activate()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                TempData["Error"] = "الرجاء إدخال الكود.";
                return View();
            }

            var result = await _enrollmentService.ActivateCodeAsync(code, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction("MyCourses"); // هيوديه لصفحة كورساته
            }
            else
            {
                TempData["Error"] = result.Message;
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var enrollments = await _enrollmentService.GetStudentEnrollmentsAsync(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            // تحويل للـ ViewModel
            var viewModel = new MyEnrollmentsViewModel
            {
                Enrollments = enrollments.Select(e => new EnrollmentItemViewModel
                {
                    CourseId = e.CourseId,
                    CourseTitle = e.Course?.Title ?? "غير معروف",
                    InstructorName = e.Course?.Instructor?.User?.FullName ?? "غير معروف",
                    EnrolledAt = e.EnrolledAt,
                    ExpiresAt = e.ExpiresAt,
                    Status = e.Status == EnrollmentStatus.Active ? "نشط" : "منتهي",
                    IsExpired = e.Status == EnrollmentStatus.Expired,
                    DaysRemaining = e.ExpiresAt > DateTime.UtcNow ? (e.ExpiresAt - DateTime.UtcNow).Days : 0
                }).ToList()
            };

            return View(viewModel);
        }
    }
}