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




        // GET: /Student/Student/CourseDetails/5
        public async Task<IActionResult> CourseDetails(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return NotFound();
            }

            // نتأكد إن الطالب مشترك في الكورس ده
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                        .ThenInclude(i => i.User)
                .FirstOrDefaultAsync(e => e.StudentId == student.Id &&
                                          e.CourseId == id &&
                                          e.Status == EnrollmentStatus.Active);

            if (enrollment == null)
            {
                return Unauthorized("أنت غير مشترك في هذا الكورس");
            }

            // نجيب Chapters مع الفيديوهات
            var chapters = await _context.Chapters
                .Include(ch => ch.Videos)
                .Where(ch => ch.CourseId == id)
                .OrderBy(ch => ch.Order)
                .ToListAsync();

            // حساب إجمالي الفيديوهات والمشاهدة
            var allVideos = chapters.SelectMany(ch => ch.Videos ?? new List<Video>()).ToList();
            var totalVideos = allVideos.Count;

            var watchedVideos = await _context.VideoProgresses
                .CountAsync(vp => vp.StudentId == student.Id &&
                                  allVideos.Select(v => v.Id).Contains(vp.VideoId) &&
                                  vp.IsCompleted);

            var progress = totalVideos > 0 ? (watchedVideos * 100) / totalVideos : 0;

            // تجهيز الـ ViewModel
            var chapterViewModels = new List<ChapterViewModel>();

            foreach (var chapter in chapters)
            {
                var videoItems = new List<VideoItemViewModel>();

                // ✅ التعديل هنا - إزالة الـ '??' مع OrderBy
                var videosInChapter = chapter.Videos ?? new List<Video>();
                var orderedVideos = videosInChapter.OrderBy(v => v.Order);

                foreach (var video in orderedVideos)
                {
                    // نشوف الفيديو اتشاف ولا لأ
                    var progressRecord = await _context.VideoProgresses
                        .FirstOrDefaultAsync(vp => vp.StudentId == student.Id && vp.VideoId == video.Id);

                    // نجيب الكويز بتاع الفيديو لو موجود
                    var quiz = await _context.Quizzes
                        .FirstOrDefaultAsync(q => q.VideoId == video.Id);

                    // نشوف الفيديو مقفول ولا لأ
                    bool isLocked = false;

                    // لو مش أول فيديو في الباب
                    if (video.Order > 1)
                    {
                        // بنجيب الفيديو اللي قبله
                        var previousVideo = await _context.Videos
                            .Where(v => v.ChapterId == video.ChapterId && v.Order == video.Order - 1)
                            .FirstOrDefaultAsync();

                        if (previousVideo != null)
                        {
                            var previousWatched = await _context.VideoProgresses
                                .AnyAsync(vp => vp.StudentId == student.Id &&
                                               vp.VideoId == previousVideo.Id &&
                                               vp.IsCompleted);

                            // لو الفيديو اللي قبله مش متشاف، يبقى مقفول
                            if (!previousWatched)
                            {
                                isLocked = true;
                            }
                        }
                    }

                    videoItems.Add(new VideoItemViewModel
                    {
                        VideoId = video.Id,
                        Title = video.Title,
                        Description = video.Description,
                        ThumbnailUrl = video.ThumbnailUrl ?? "/images/video-default.jpg",
                        DurationSeconds = video.DurationSeconds,
                        Order = video.Order,
                        IsWatched = progressRecord?.IsCompleted ?? false,
                        IsLocked = isLocked,
                        HasQuiz = quiz != null,
                        QuizId = quiz?.Id,
                        LastPosition = progressRecord?.LastPosition,
                        YouTubeUrl = video.YouTubeUrl
                    });
                }

                chapterViewModels.Add(new ChapterViewModel
                {
                    ChapterId = chapter.Id,
                    Title = chapter.Title,
                    Order = chapter.Order,
                    Videos = videoItems
                });
            }

            var viewModel = new CourseDetailsViewModel
            {
                CourseId = enrollment.Course.Id,
                CourseTitle = enrollment.Course.Title,
                CourseDescription = enrollment.Course.Description,
                InstructorName = enrollment.Course.Instructor?.User?.FullName ?? "غير معروف",
                InstructorBio = enrollment.Course.Instructor?.Bio,
                EnrolledAt = enrollment.EnrolledAt,
                ExpiresAt = enrollment.ExpiresAt,
                TotalVideos = totalVideos,
                WatchedVideos = watchedVideos,
                ProgressPercentage = progress,
                Chapters = chapterViewModels
            };

            return View(viewModel);
        }



        // GET: /Student/Student/WatchVideo/5
        public async Task<IActionResult> WatchVideo(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                return NotFound();
            }

            // نجيب الفيديو مع الكورس بتاعه
            var video = await _context.Videos
                .Include(v => v.Chapter)
                    .ThenInclude(ch => ch.Course)
                        .ThenInclude(c => c.Instructor)
                            .ThenInclude(i => i.User)
                .Include(v => v.AttachedFiles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
            {
                return NotFound();
            }

            // نتأكد إن الطالب مشترك في الكورس
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == student.Id &&
                                          e.CourseId == video.Chapter.CourseId &&
                                          e.Status == EnrollmentStatus.Active);

            if (enrollment == null)
            {
                return Unauthorized("أنت غير مشترك في هذا الكورس");
            }

            // نجيب الفيديو اللي قبله واللي بعده
            var previousVideo = await _context.Videos
                .Where(v => v.ChapterId == video.ChapterId && v.Order == video.Order - 1)
                .FirstOrDefaultAsync();

            var nextVideo = await _context.Videos
                .Where(v => v.ChapterId == video.ChapterId && v.Order == video.Order + 1)
                .FirstOrDefaultAsync();

            // نجيب تقدم الطالب في الفيديو ده
            var progress = await _context.VideoProgresses
                .FirstOrDefaultAsync(vp => vp.StudentId == student.Id && vp.VideoId == video.Id);

            // نجيب الكويز بتاع الفيديو لو موجود
            var videoQuiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.VideoId == video.Id);

            // نشوف لو الطالب عدى الكويز ده قبل كده
            bool quizPassed = false;
            if (videoQuiz != null)
            {
                quizPassed = await _context.QuizAttempts
                    .AnyAsync(qa => qa.QuizId == videoQuiz.Id &&
                                   qa.StudentId == student.Id &&
                                   qa.Passed);
            }

            // تحويل الملفات المرفقة
            var attachedFiles = video.AttachedFiles?.Select(f => new AttachedFileViewModel
            {
                FileId = f.Id,
                FileName = f.FileName,
                FilePath = f.FilePath,
                FileType = f.FileType.ToString(),
                FileSize = f.FileSize,
                FormattedFileSize = FormatFileSize(f.FileSize)
            }).ToList() ?? new List<AttachedFileViewModel>();

            // استخراج YouTube Embed URL
            string embedUrl = ExtractYouTubeEmbedUrl(video.YouTubeUrl);

            var viewModel = new VideoPlayerViewModel
            {
                VideoId = video.Id,
                Title = video.Title,
                Description = video.Description,
                YouTubeUrl = video.YouTubeUrl,
                YouTubeEmbedUrl = embedUrl,
                DurationSeconds = video.DurationSeconds,

                CourseId = video.Chapter.CourseId,
                CourseTitle = video.Chapter.Course.Title,

                ChapterId = video.ChapterId,
                ChapterTitle = video.Chapter.Title,

                PreviousVideoId = previousVideo?.Id,
                NextVideoId = nextVideo?.Id,

                AttachedFiles = attachedFiles,

                HasQuiz = videoQuiz != null,
                QuizId = videoQuiz?.Id,
                QuizPassed = quizPassed,

                LastPosition = progress?.LastPosition ?? 0,
                IsCompleted = progress?.IsCompleted ?? false
            };

            return View(viewModel);
        }




        // POST: /Student/Student/UpdateVideoProgress
        [HttpPost]
        public async Task<IActionResult> UpdateVideoProgress([FromBody] UpdateProgressViewModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (student == null)
                {
                    return Json(new { success = false });
                }

                var progress = await _context.VideoProgresses
                    .FirstOrDefaultAsync(vp => vp.StudentId == student.Id && vp.VideoId == model.VideoId);

                if (progress == null)
                {
                    progress = new VideoProgress
                    {
                        VideoId = model.VideoId,
                        StudentId = student.Id,
                        LastPosition = model.Position,
                        IsCompleted = model.IsCompleted,
                        LastWatchedAt = DateTime.Now
                    };
                    _context.VideoProgresses.Add(progress);
                }
                else
                {
                    progress.LastPosition = model.Position;
                    if (model.IsCompleted && !progress.IsCompleted)
                    {
                        progress.IsCompleted = true;
                    }
                    progress.LastWatchedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث تقدم الفيديو");
                return Json(new { success = false });
            }
        }

        // GET: /Student/Student/DownloadFile/5
        public async Task<IActionResult> DownloadFile(int id)
        {
            var file = await _context.AttachedFiles.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            var path = Path.Combine(Directory.GetCurrentDirectory(), file.FilePath);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(path), file.FileName);
        }

        // Helper Methods
        private string ExtractYouTubeEmbedUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";

            // https://youtu.be/VIDEO_ID
            // https://www.youtube.com/watch?v=VIDEO_ID

            string videoId = "";

            if (url.Contains("youtu.be"))
            {
                videoId = url.Split('/').Last().Split('?').First();
            }
            else if (url.Contains("youtube.com/watch"))
            {
                var query = System.Web.HttpUtility.ParseQueryString(new Uri(url).Query);
                videoId = query["v"];
            }

            return $"https://www.youtube.com/embed/{videoId}?enablejsapi=1&autoplay=1";
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
    {
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".mp4", "video/mp4" }
    };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }
    }
}