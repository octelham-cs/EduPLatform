using System.Threading.Tasks;
using EduPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult MyCourses()
        {
            // هنا هنجيب الكورسات اللي الطالب مشترك فيها
            // هنتعامل معاها في خطوة تانية
            return View();
        }
    }
}