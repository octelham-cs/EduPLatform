using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EduPlatform.Core.Entities;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Web.ViewModels.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);

            var model = new CreateQuizViewModel
            {
                // جلب كورسات المدرس
                Courses = await _context.Courses
                    .Where(c => c.InstructorId == instructor.Id)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title })
                    .ToListAsync(),

                // جلب كل الأسئلة (أو ممكن نفلتر بعدين)
                AvailableQuestions = await _context.Questions
                    .Include(q => q.Subject)
                    .Select(q => new QuestionListItem { Id = q.Id, QuestionText = q.QuestionText, SubjectName = q.Subject.Name })
                    .ToListAsync()
            };

            return View(model);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateQuizViewModel model)
        {
            if (!ModelState.IsValid || model.SelectedQuestionIds == null || !model.SelectedQuestionIds.Any())
            {
                TempData["Error"] = "يجب اختيار مادة وأسئلة للاختبار.";
                return RedirectToAction("Create"); // ممكن نعيد تحميل الموديل
            }

            var quiz = new Quiz
            {
                Title = model.Title,
                CourseId = model.CourseId,
                Type = model.Type,
                TimeLimit = model.TimeLimit,
                PassingScore = model.PassingScore,
                QuestionsJson = JsonSerializer.Serialize(model.SelectedQuestionIds), // حفظ الـ IDs
                CreatedAt = DateTime.UtcNow
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إنشاء الاختبار بنجاح!";
            return RedirectToAction("Index", "Dashboard", new { area = "Instructor" });
        }
    }
}