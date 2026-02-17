using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using EduPlatform.Core.Interfaces;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Web.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EduPlatform.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEnrollmentService _enrollmentService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            ApplicationDbContext context,
            IEnrollmentService enrollmentService,
            ILogger<StudentController> logger)
        {
            _context = context;
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        // GET: /Student/Student/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // نجيب الطالب الحالي
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return NotFound("الطالب غير موجود");
            }

            // نجيب الاشتراكات النشطة
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.User)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Chapters)
                        .ThenInclude(ch => ch.Videos)
                .Where(e => e.StudentId == student.Id && e.Status == EnrollmentStatus.Active)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            var enrolledCourses = new List<EnrolledCourseViewModel>();
            var sevenDaysFromNow = DateTime.Now.AddDays(7);

            foreach (var enrollment in enrollments)
            {
                // حساب عدد الفيديوهات
                var totalVideos = enrollment.Course.Chapters?
                    .SelectMany(ch => ch.Videos ?? new List<Video>())
                    .Count() ?? 0;

                // حساب الفيديوهات المشاهدة
                var watchedVideos = await _context.VideoProgresses
                    .CountAsync(vp => vp.StudentId == student.Id &&
                                      vp.Video.Chapter.CourseId == enrollment.CourseId &&
                                      vp.IsCompleted);

                var progress = totalVideos > 0 ? (watchedVideos * 100) / totalVideos : 0;

                enrolledCourses.Add(new EnrolledCourseViewModel
                {
                    EnrollmentId = enrollment.Id,
                    CourseId = enrollment.CourseId,
                    CourseTitle = enrollment.Course.Title,
                    CourseDescription = enrollment.Course.Description,
                    InstructorName = enrollment.Course.Instructor?.User?.FullName ?? "غير معروف",
                    ThumbnailUrl = "/images/course-default.jpg", // هنضيف thumbnails بعدين
                    Price = enrollment.EnrollmentCode?.Price ?? 0,
                    EnrolledAt = enrollment.EnrolledAt,
                    ExpiresAt = enrollment.ExpiresAt,
                    TotalVideos = totalVideos,
                    WatchedVideos = watchedVideos,
                    ProgressPercentage = progress,
                    IsActive = enrollment.Status == EnrollmentStatus.Active,
                    IsExpiringSoon = enrollment.ExpiresAt <= sevenDaysFromNow && enrollment.ExpiresAt > DateTime.Now
                });
            }

            var viewModel = new StudentDashboardViewModel
            {
                EnrolledCourses = enrolledCourses,
                TotalEnrollments = enrollments.Count,
                ActiveEnrollments = enrollments.Count(e => e.Status == EnrollmentStatus.Active),
                CompletedCourses = enrollments.Count(e => e.Status == EnrollmentStatus.Expired)
            };

            return View(viewModel);
        }

        // POST: /Student/Student/ActivateCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateCode([FromBody] ActivateCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new ActivateCodeResultViewModel
                {
                    Success = false,
                    Message = "الكود غير صالح"
                });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (student == null)
                {
                    return Json(new ActivateCodeResultViewModel
                    {
                        Success = false,
                        Message = "الطالب غير موجود"
                    });
                }

                var result = await _enrollmentService.ActivateCodeAsync(model.Code, student.Id.ToString());

                if (result.Success)
                {
                    // ✅ نجيب آخر اشتراك للطالب
                    var enrollment = await _context.Enrollments
                        .Include(e => e.Course)
                            .ThenInclude(c => c.Instructor)
                                .ThenInclude(i => i.User)
                        .Where(e => e.StudentId == student.Id)
                        .OrderByDescending(e => e.EnrolledAt)
                        .FirstOrDefaultAsync();

                    return Json(new ActivateCodeResultViewModel
                    {
                        Success = true,
                        Message = "تم تفعيل الكود بنجاح",
                        CourseName = enrollment?.Course?.Title,
                        InstructorName = enrollment?.Course?.Instructor?.User?.FullName,
                        ExpiresAt = enrollment?.ExpiresAt ?? DateTime.Now,
                        CourseId = enrollment?.CourseId ?? 0
                    });
                }
                else
                {
                    return Json(new ActivateCodeResultViewModel
                    {
                        Success = false,
                        Message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تفعيل الكود");
                return Json(new ActivateCodeResultViewModel
                {
                    Success = false,
                    Message = "حدث خطأ أثناء تفعيل الكود"
                });
            }
        }





        // GET: /Student/Student/MyEnrollments
        public async Task<IActionResult> MyEnrollments(string status = "active")
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return NotFound();
            }

            var query = _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.User)
                .Where(e => e.StudentId == student.Id)
                .AsQueryable();

            if (status == "active")
            {
                query = query.Where(e => e.Status == EnrollmentStatus.Active);
            }
            else if (status == "expired")
            {
                query = query.Where(e => e.Status == EnrollmentStatus.Expired);
            }

            var enrollments = await query
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            // TODO: Map to ViewModel
            return View(enrollments);
        }
    }
}