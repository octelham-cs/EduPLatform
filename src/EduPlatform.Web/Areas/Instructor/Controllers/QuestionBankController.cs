// src/EduPlatform.Web/Areas/Instructor/Controllers/QuestionBankController.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using EduPlatform.Infrastructure.Data;
using EduPlatform.Web.ViewModels.Instructor;
using Microsoft.AspNetCore.Identity;

namespace EduPlatform.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class QuestionBankController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public QuestionBankController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Instructor/QuestionBank
        public async Task<IActionResult> Index()
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

            var questions = await _context.Questions
                .Include(q => q.Subject)
                .Include(q => q.Chapter)
                .Where(q => q.InstructorId == instructor.Id)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return View(questions);
        }

        // GET: /Instructor/QuestionBank/Create
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            await LoadDropdowns(instructor?.Id);

            return View(new AddQuestionViewModel());
        }

        // POST: /Instructor/QuestionBank/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(instructor.Id);
                return View(model);
            }

            var question = new Question
            {
                InstructorId = instructor.Id,
                SubjectId = model.SubjectId,
                ChapterId = model.ChapterId,
                QuestionText = model.QuestionText,
                Type = Enum.Parse<QuestionType>(model.QuestionType),
                DifficultyLevel = model.DifficultyLevel,
                CreatedAt = DateTime.Now
            };

            // تحويل الاختيارات والإجابة لـ JSON
            if (model.QuestionType == "MultipleChoice" || model.QuestionType == "MultipleSelect")
            {
                question.OptionsJson = JsonSerializer.Serialize(model.Options.Where(o => !string.IsNullOrEmpty(o)).ToList());
                question.CorrectAnswerJson = JsonSerializer.Serialize(new { index = model.CorrectAnswerIndex });
            }
            else if (model.QuestionType == "TrueFalse")
            {
                question.OptionsJson = JsonSerializer.Serialize(new List<string> { "صح", "خطأ" });
                question.CorrectAnswerJson = JsonSerializer.Serialize(new { answer = model.CorrectAnswerBool });
            }

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم إضافة السؤال بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Instructor/QuestionBank/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id && q.InstructorId == instructor!.Id);

            if (question == null) return NotFound();

            var model = new AddQuestionViewModel
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.Type.ToString(),
                SubjectId = question.SubjectId,
                ChapterId = question.ChapterId,
                DifficultyLevel = question.DifficultyLevel
            };

            // تحميل الاختيارات من JSON
            if (!string.IsNullOrEmpty(question.OptionsJson))
            {
                var options = JsonSerializer.Deserialize<List<string>>(question.OptionsJson);
                if (options != null)
                {
                    model.Options = options;
                    // إضافة اختيارات فارغة للوصول لـ 4
                    while (model.Options.Count < 4) model.Options.Add("");
                }
            }

            await LoadDropdowns(instructor!.Id);
            return View(model);
        }

        // POST: /Instructor/QuestionBank/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddQuestionViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id && q.InstructorId == instructor!.Id);

            if (question == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(instructor.Id);
                return View(model);
            }

            question.QuestionText = model.QuestionText;
            question.Type = Enum.Parse<QuestionType>(model.QuestionType);
            question.SubjectId = model.SubjectId;
            question.ChapterId = model.ChapterId;
            question.DifficultyLevel = model.DifficultyLevel;

            if (model.QuestionType == "MultipleChoice" || model.QuestionType == "MultipleSelect")
            {
                question.OptionsJson = JsonSerializer.Serialize(model.Options.Where(o => !string.IsNullOrEmpty(o)).ToList());
                question.CorrectAnswerJson = JsonSerializer.Serialize(new { index = model.CorrectAnswerIndex });
            }
            else if (model.QuestionType == "TrueFalse")
            {
                question.OptionsJson = JsonSerializer.Serialize(new List<string> { "صح", "خطأ" });
                question.CorrectAnswerJson = JsonSerializer.Serialize(new { answer = model.CorrectAnswerBool });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث السؤال بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Instructor/QuestionBank/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var question = await _context.Questions
                .FirstOrDefaultAsync(q => q.Id == id && q.InstructorId == instructor!.Id);

            if (question == null) return NotFound();

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تم حذف السؤال بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdowns(int? instructorId)
        {
            var subjects = await _context.Subjects
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.Subjects = new SelectList(subjects, "Id", "Name");

            if (instructorId.HasValue)
            {
                var chapters = await _context.Chapters
                    .Include(c => c.Course)
                    .Where(c => c.Course.InstructorId == instructorId)
                    .Select(c => new { c.Id, Display = c.Course.Title + " - " + c.Title })
                    .ToListAsync();

                ViewBag.Chapters = new SelectList(chapters, "Id", "Display");
            }
        }
    }
}