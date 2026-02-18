// src/EduPlatform.Web/Areas/Instructor/Controllers/CourseController.cs

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
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Instructor/Course
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

            var courses = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Chapters)
                .Where(c => c.InstructorId == instructor.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(courses);
        }

        // GET: /Instructor/Course/Create
        public async Task<IActionResult> Create()
        {
            await LoadSubjectsDropdown();
            return View(new AddCourseViewModel());
        }

        // POST: /Instructor/Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddCourseViewModel model)
        {


            
        











            if (!ModelState.IsValid)
            {
                await LoadSubjectsDropdown();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            if (instructor == null)
            {
                TempData["Error"] = "لم يتم العثور على حساب المدرس";
                return RedirectToAction("Index", "Dashboard");
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

            TempData["Success"] = "تم إضافة الكورس بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Instructor/Course/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var course = await _context.Courses
                .Include(c => c.Subject)
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Videos)
                .FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == instructor!.Id);

            if (course == null) return NotFound();

            return View(course);
        }

        // GET: /Instructor/Course/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == instructor!.Id);

            if (course == null) return NotFound();

            var viewModel = new AddCourseViewModel
            {
                SubjectId = course.SubjectId,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price
            };

            await LoadSubjectsDropdown();
            return View(viewModel);
        }

        // POST: /Instructor/Course/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadSubjectsDropdown();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(i => i.UserId == user.Id);

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == id && c.InstructorId == instructor!.Id);

            if (course == null) return NotFound();

            course.SubjectId = model.SubjectId;
            course.Title = model.Title;
            course.Description = model.Description;
            course.Price = model.Price;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تم تحديث الكورس بنجاح!";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadSubjectsDropdown()
        {
            var subjects = await _context.Subjects
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.Subjects = new SelectList(subjects, "Id", "Name");
        }
    }
}