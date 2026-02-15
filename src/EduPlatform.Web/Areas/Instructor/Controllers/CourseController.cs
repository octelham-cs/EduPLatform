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
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ========================================
        // GET: Instructor/Course
        // ========================================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null || instructor.Status != InstructorStatus.Approved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Where(c => c.InstructorId == instructor.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(courses);
        }

        // ========================================
        // GET: Instructor/Course/Create
        // ========================================
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null || instructor.Status != InstructorStatus.Approved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            // جلب المواد المتاحة
            ViewBag.Subjects = await _context.Subjects
                .Include(s => s.GradeLevelId)
                .Where(s => s.IsActive)
                .OrderBy(s => s.GradeLevelId)
                .ToListAsync();

            return View();
        }

        // ========================================
        // POST: Instructor/Course/Create
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddCourseViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null || instructor.Status != InstructorStatus.Approved)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Subjects = await _context.Subjects
                    .Include(s => s.GradeLevelId)
                    .Where(s => s.IsActive)
                    .ToListAsync();
                return View(model);
            }

            var course = new Course
            {
                InstructorId = instructor.Id,
                SubjectId = model.SubjectId,
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إنشاء الكورس بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // ========================================
        // GET: Instructor/Course/Details/5
        // ========================================
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Videos)
                .FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == instructor!.Id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // ========================================
        // POST: Instructor/Course/AddChapter
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChapter(AddChapterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "حدث خطأ في البيانات";
                return RedirectToAction(nameof(Details), new { id = model.CourseId });
            }

            var chapter = new Chapter
            {
                CourseId = model.CourseId,
                Title = model.Title,
                Description = model.Description,
                Order = model.Order,
                IsActive = true
            };

            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الفصل بنجاح";
            return RedirectToAction(nameof(Details), new { id = model.CourseId });
        }

        // ========================================
        // POST: Instructor/Course/AddVideo
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVideo(AddVideoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "حدث خطأ في البيانات";
                return RedirectToAction(nameof(Details), new { id = model.CourseId });
            }

            // استخراج معرف يوتيوب من الرابط
            var youtubeId = ExtractYouTubeId(model.YouTubeUrl);

            var video = new Video
            {
                ChapterId = model.ChapterId,
                Title = model.Title,
                Description = model.Description,
                YouTubeUrl = model.YouTubeUrl,
                YouTubeVideoId = youtubeId,
                ThumbnailUrl = GetThumbnailUrl(youtubeId),
                Order = model.Order,
                CreatedAt = DateTime.Now
            };

            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الفيديو بنجاح";
            return RedirectToAction(nameof(Details), new { id = model.CourseId });
        }

        // ========================================
        // POST: Instructor/Course/HideVideo/5
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HideVideo(int id, int courseId)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video != null)
            {
                video.IsHidden = !video.IsHidden;
                await _context.SaveChangesAsync();
                TempData["Success"] = video.IsHidden ? "تم إخفاء الفيديو" : "تم إظهار الفيديو";
            }
            return RedirectToAction(nameof(Details), new { id = courseId });
        }

        // ========================================
        // Helper: استخراج معرف يوتيوب
        // ========================================
        private string? ExtractYouTubeId(string url)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            // أنماط مختلفة لروابط يوتيوب
            var patterns = new[]
            {
                @"(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([^&\?\/]+)",
                @"youtube\.com\/v\/([^&\?\/]+)"
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(url, pattern);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            return null;
        }

        // ========================================
        // Helper: الحصول على صورة مصغرة
        // ========================================
        private string? GetThumbnailUrl(string? youtubeId)
        {
            if (string.IsNullOrEmpty(youtubeId))
                return null;

            return $"https://img.youtube.com/vi/{youtubeId}/mqdefault.jpg";
        }
    }
}