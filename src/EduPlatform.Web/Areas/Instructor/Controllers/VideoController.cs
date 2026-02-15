// src/EduPlatform.Web/Areas/Instructor/Controllers/VideoController.cs

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduPlatform.Core.Entities;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Web.ViewModels.Instructor;
using Microsoft.AspNetCore.Identity;

namespace EduPlatform.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class VideoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public VideoController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Instructor/Video?courseId=5
        public async Task<IActionResult> Index(int? courseId)
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

            // تحميل كورسات المدرس
            var courses = await _context.Courses
                .Where(c => c.InstructorId == instructor.Id && c.IsActive)
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Videos)
                .ToListAsync();

            if (courseId.HasValue)
            {
                var selectedCourse = courses.FirstOrDefault(c => c.Id == courseId);
                ViewBag.SelectedCourse = selectedCourse;
            }

            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;

            return View();
        }

        // GET: /Instructor/Video/Create?chapterId=5
        public async Task<IActionResult> Create(int? chapterId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null) return NotFound();

            await LoadChaptersDropdown(instructor.Id);

            var model = new AddVideoViewModel();

            if (chapterId.HasValue)
            {
                model.ChapterId = chapterId.Value;
            }

            return View(model);
        }

        // POST: /Instructor/Video/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddVideoViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadChaptersDropdown(instructor.Id);
                return View(model);
            }

            // استخراج Thumbnail من YouTube
            var thumbnailUrl = GetYouTubeThumbnail(model.YouTubeUrl);

            var video = new Video
            {
                ChapterId = model.ChapterId,
                Title = model.Title,
                Description = model.Description,
                YouTubeUrl = model.YouTubeUrl,
                ThumbnailUrl = thumbnailUrl,
                Order = model.Order,
                IsHidden = model.IsHidden,
                CreatedAt = DateTime.Now
            };

            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة الفيديو بنجاح!";
            return RedirectToAction(nameof(Index), new { courseId = GetCourseIdByChapter(model.ChapterId) });
        }

        // GET: /Instructor/Video/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var video = await _context.Videos
                .Include(v => v.Chapter)
                    .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(v => v.Id == id && v.Chapter!.Course.InstructorId == instructor!.Id);

            if (video == null) return NotFound();

            await LoadChaptersDropdown(instructor!.Id);

            var model = new AddVideoViewModel
            {
                Id = video.Id,
                ChapterId = video.ChapterId,
                Title = video.Title,
                Description = video.Description,
                YouTubeUrl = video.YouTubeUrl,
                Order = video.Order,
                IsHidden = video.IsHidden
            };

            return View(model);
        }

        // POST: /Instructor/Video/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddVideoViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var video = await _context.Videos
                .Include(v => v.Chapter)
                    .ThenInclude(c => c.Course)
                .FirstOrDefaultAsync(v => v.Id == id && v.Chapter!.Course.InstructorId == instructor!.Id);

            if (video == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadChaptersDropdown(instructor!.Id);
                return View(model);
            }

            video.ChapterId = model.ChapterId;
            video.Title = model.Title;
            video.Description = model.Description;
            video.YouTubeUrl = model.YouTubeUrl;
            video.ThumbnailUrl = GetYouTubeThumbnail(model.YouTubeUrl);
            video.Order = model.Order;
            video.IsHidden = model.IsHidden;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث الفيديو بنجاح!";
            return RedirectToAction(nameof(Index), new { courseId = GetCourseIdByChapter(model.ChapterId) });
        }

        // POST: /Instructor/Video/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var video = await _context.Videos
                .Include(v => v.Chapter)
                .FirstOrDefaultAsync(v => v.Id == id && v.Chapter!.Course.InstructorId == instructor!.Id);

            if (video == null) return NotFound();

            _context.Videos.Remove(video);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الفيديو بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Instructor/Video/ToggleVisibility/5
        [HttpPost]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var video = await _context.Videos
                .Include(v => v.Chapter)
                .FirstOrDefaultAsync(v => v.Id == id && v.Chapter!.Course.InstructorId == instructor!.Id);

            if (video == null) return NotFound();

            video.IsHidden = !video.IsHidden;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isHidden = video.IsHidden });
        }

        private async Task LoadChaptersDropdown(int instructorId)
        {
            var chapters = await _context.Chapters
                .Include(c => c.Course)
                .Where(c => c.Course.InstructorId == instructorId)
                .OrderBy(c => c.Course.Title)
                .ThenBy(c => c.Order)
                .Select(c => new { c.Id, Display = c.Course.Title + " - " + c.Title })
                .ToListAsync();

            ViewBag.Chapters = new SelectList(chapters, "Id", "Display");
        }

        private int? GetCourseIdByChapter(int chapterId)
        {
            var chapter = _context.Chapters
                .FirstOrDefault(c => c.Id == chapterId);
            return chapter?.CourseId;
        }

        private string GetYouTubeThumbnail(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";

            var videoId = "";

            // YouTube URL formats
            if (url.Contains("youtube.com/watch?v="))
            {
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                videoId = query["v"] ?? "";
            }
            else if (url.Contains("youtu.be/"))
            {
                videoId = url.Substring(url.LastIndexOf("/") + 1);
            }
            else if (url.Contains("youtube.com/embed/"))
            {
                var parts = url.Split('/');
                videoId = parts.Last().Split('?')[0];
            }

            return string.IsNullOrEmpty(videoId) ? "" : $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";
        }
    }
}