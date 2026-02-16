using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
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

        // GET: /Instructor/Quiz
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);
            if (instructor == null) return NotFound();

            var quizzes = await _context.Quizzes
                .Include(q => q.Course)
                .Where(q => q.Course.InstructorId == instructor.Id)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            var viewModel = quizzes.Select(q =>
            {
                var questionIds = JsonSerializer.Deserialize<List<int>>(q.QuestionsJson ?? "[]") ?? new List<int>();
                var attempts = _context.QuizAttempts.Count(a => a.QuizId == q.Id);

                return new QuizViewModel
                {
                    Id = q.Id,
                    Title = q.Title,
                    CourseName = q.Course?.Title ?? "غير محدد",
                    Type = q.Type == QuizType.Progress ? "Progress" : "Assessment",
                    TimeLimit = q.TimeLimit,
                    PassingScore = q.PassingScore,
                    QuestionsCount = questionIds.Count,
                    AttemptsCount = attempts
                };
            }).ToList();

            return View(viewModel);
        }

        // GET: /Instructor/Quiz/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);
            if (instructor == null) return NotFound();

            var model = new CreateQuizViewModel
            {
                Courses = await _context.Courses
                    .Where(c => c.InstructorId == instructor.Id)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title })
                    .ToListAsync(),

                AvailableQuestions = await _context.Questions
                    .Include(q => q.Subject)
                    .Where(q => q.InstructorId == instructor.Id)
                    .Select(q => new QuestionListItem
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        SubjectName = q.Subject.Name
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        // POST: /Instructor/Quiz/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateQuizViewModel model)
        {
            if (!ModelState.IsValid || model.SelectedQuestionIds == null || !model.SelectedQuestionIds.Any())
            {
                TempData["Error"] = "يجب اختيار مادة وأسئلة للاختبار.";
                return RedirectToAction("Create");
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);
            if (instructor == null) return NotFound();

            var quiz = new Quiz
            {
                Title = model.Title,
                CourseId = model.CourseId,
                Type = model.Type,
                TimeLimit = model.TimeLimit,
                PassingScore = model.PassingScore,
                QuestionsJson = JsonSerializer.Serialize(model.SelectedQuestionIds),
                CreatedAt = DateTime.UtcNow
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إنشاء الاختبار بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Instructor/Quiz/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);
            if (instructor == null) return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == id && q.Course.InstructorId == instructor.Id);

            if (quiz == null) return NotFound();

            var questionIds = JsonSerializer.Deserialize<List<int>>(quiz.QuestionsJson ?? "[]") ?? new List<int>();

            var model = new CreateQuizViewModel
            {
                Id = quiz.Id,
                Title = quiz.Title,
                CourseId = quiz.CourseId,
                Type = quiz.Type,
                TimeLimit = quiz.TimeLimit,
                PassingScore = quiz.PassingScore,
                SelectedQuestionIds = questionIds,

                Courses = await _context.Courses
                    .Where(c => c.InstructorId == instructor.Id)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Title })
                    .ToListAsync(),

                AvailableQuestions = await _context.Questions
                    .Include(q => q.Subject)
                    .Where(q => q.InstructorId == instructor.Id)
                    .Select(q => new QuestionListItem
                    {
                        Id = q.Id,
                        QuestionText = q.QuestionText,
                        SubjectName = q.Subject.Name
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        // POST: /Instructor/Quiz/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateQuizViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid || model.SelectedQuestionIds == null || !model.SelectedQuestionIds.Any())
            {
                TempData["Error"] = "يجب اختيار مادة وأسئلة للاختبار.";
                return RedirectToAction("Edit", new { id });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);
            if (instructor == null) return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == id && q.Course.InstructorId == instructor.Id);

            if (quiz == null) return NotFound();

            quiz.Title = model.Title;
            quiz.CourseId = model.CourseId;
            quiz.Type = model.Type;
            quiz.TimeLimit = model.TimeLimit;
            quiz.PassingScore = model.PassingScore;
            quiz.QuestionsJson = JsonSerializer.Serialize(model.SelectedQuestionIds);

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تعديل الاختبار بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Instructor/Quiz/Results/5
        public async Task<IActionResult> Results(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);
            if (instructor == null) return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == id && q.Course.InstructorId == instructor.Id);

            if (quiz == null) return NotFound();

            var attempts = await _context.QuizAttempts
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Where(a => a.QuizId == id)
                .OrderByDescending(a => a.FinishedAt)
                .ToListAsync();

            var viewModel = new QuizResultsViewModel
            {
                QuizId = quiz.Id,
                QuizTitle = quiz.Title,
                TotalAttempts = attempts.Count,
                PassedCount = attempts.Count(a => a.Passed),
                FailedCount = attempts.Count(a => !a.Passed),
                AverageScore = attempts.Any() ? attempts.Average(a => a.Score) : 0,
                Attempts = attempts.Select(a => new QuizAttemptViewModel
                {
                    StudentName = a.Student?.User?.FullName ?? "غير معروف",
                    StudentEmail = a.Student?.User?.Email ?? "",
                    AttemptDate = a.FinishedAt,
                    Score = a.Score,
                    Passed = a.Passed,
                    TimeTaken = (int)(a.FinishedAt - a.StartedAt).TotalMinutes
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /Instructor/Quiz/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);
            if (instructor == null) return NotFound();

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == id && q.Course.InstructorId == instructor.Id);

            if (quiz == null) return NotFound();

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف الاختبار بنجاح!";
            return RedirectToAction(nameof(Index));
        }
    }
}