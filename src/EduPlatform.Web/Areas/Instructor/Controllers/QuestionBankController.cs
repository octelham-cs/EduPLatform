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
    public class QuestionBankController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionBankController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            var questions = await _context.Questions
                .Include(q => q.Subject)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(questions);
        }

        // GET: Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new QuestionViewModel
            {
                Subjects = await _context.Subjects
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                    .ToListAsync()
            };
            return View(viewModel);
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(QuestionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Subjects = await _context.Subjects
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                    .ToListAsync();
                return View(model);
            }

            // تجهيز السؤال
            var question = new Question
            {
                QuestionText = model.QuestionText,
                Type = model.Type,
                SubjectId = model.SubjectId,
                ChapterId = model.ChapterId,
                DifficultyLevel = model.DifficultyLevel,
                CreatedAt = DateTime.UtcNow,
                CorrectAnswerJson = model.CorrectAnswer
            };

            // لو اختيار من متعدد، نحفظ الاختيارات كـ JSON
            if (model.Type == QuestionType.MultipleChoice)
            {
                question.OptionsJson = JsonSerializer.Serialize(model.Options);
            }

            // ربط السؤال بالمدرس الحالي
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var instructor = await _context.Instructors.FirstOrDefaultAsync(i => i.UserId == userId);

            if (instructor == null) return Unauthorized();

            question.InstructorId = instructor.Id;

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة السؤال بنجاح!";
            return RedirectToAction("Index");
        }
    }
}