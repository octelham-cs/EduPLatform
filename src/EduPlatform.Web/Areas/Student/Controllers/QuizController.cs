using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using EduPlatform.Core.Entities;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Web.ViewModels.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduPlatform.Web.Areas.Student.Controllers
{
    [Area("Student")]
    [Authorize(Roles = "Student")]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TakeQuiz
        public async Task<IActionResult> Take(int id) // id = QuizId
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null) return NotFound();

            // جلب الأسئلة من الـ JSON
            var questionIds = JsonSerializer.Deserialize<List<int>>(quiz.QuestionsJson);
            var questions = await _context.Questions
                .Where(q => questionIds.Contains(q.Id))
                .ToListAsync();

            var model = new TakeQuizViewModel
            {
                QuizId = quiz.Id,
                Title = quiz.Title,
                TimeLimit = quiz.TimeLimit,
                Questions = questions
            };

            return View(model);
        }

        // POST: Submit
        [HttpPost]
        public async Task<IActionResult> Submit(SubmitQuizViewModel model)
        {
            var quiz = await _context.Quizzes.FindAsync(model.QuizId);
            var questionIds = JsonSerializer.Deserialize<List<int>>(quiz.QuestionsJson);
            var questions = await _context.Questions.Where(q => questionIds.Contains(q.Id)).ToListAsync();

            // التصحيح
            int correctCount = 0;
            foreach (var question in questions)
            {
                if (model.Answers.TryGetValue(question.Id, out string userAnswer))
                {
                    if (question.CorrectAnswerJson == userAnswer)
                    {
                        correctCount++;
                    }
                }
            }

            // حساب الدرجة
            int totalQuestions = questions.Count;
            int score = (int)((double)correctCount / totalQuestions * 100);
            bool passed = score >= quiz.PassingScore;

            // جلب الـ Student
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);

            // حفظ المحاولة
            var attempt = new QuizAttempt
            {
                QuizId = quiz.Id,
                StudentId = student.Id,
                AnswersJson = JsonSerializer.Serialize(model.Answers),
                Score = score,
                Passed = passed,
                StartedAt = DateTime.UtcNow.AddMinutes(-quiz.TimeLimit), // تقريبي
                FinishedAt = DateTime.UtcNow
            };

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            // توجيه لصفحة النتيجة
            return RedirectToAction("Result", new { id = attempt.Id });
        }

        // GET: Result
        public async Task<IActionResult> Result(int id) // id = AttemptId
        {
            var attempt = await _context.QuizAttempts
                .Include(a => a.Quiz)
                .FirstOrDefaultAsync(a => a.Id == id);

            return View(attempt);
        }
    }
}