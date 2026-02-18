using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using EduPlatform.Core.Interfaces;
using EduPlatform.Web.ViewModels.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace EduPlatform.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TicketManagementController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketManagementController> _logger;

        public TicketManagementController(ITicketService ticketService, ILogger<TicketManagementController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        // GET: /Admin/TicketManagement
        public async Task<IActionResult> Index(TicketStatus? status = null, TicketPriority? priority = null)
        {
            ViewBag.StatusFilter = status;
            ViewBag.PriorityFilter = priority;

            var tickets = await _ticketService.GetAllTicketsAsync(status, priority);

            var viewModel = tickets.Select(t => new TicketListItemViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status switch
                {
                    TicketStatus.Open => "مفتوحة",
                    TicketStatus.InProgress => "قيد المراجعة",
                    TicketStatus.Resolved => "تم الحل",
                    TicketStatus.Closed => "مغلقة",
                    _ => "غير معروف"
                },
                StatusColor = t.Status switch
                {
                    TicketStatus.Open => "warning",
                    TicketStatus.InProgress => "info",
                    TicketStatus.Resolved => "success",
                    TicketStatus.Closed => "secondary",
                    _ => "light"
                },
                Priority = t.Priority switch
                {
                    TicketPriority.Low => "منخفضة",
                    TicketPriority.Medium => "متوسطة",
                    TicketPriority.High => "عالية",
                    TicketPriority.Urgent => "عاجلة",
                    _ => "غير معروف"
                },
                PriorityColor = t.Priority switch
                {
                    TicketPriority.Low => "success",
                    TicketPriority.Medium => "primary",
                    TicketPriority.High => "warning",
                    TicketPriority.Urgent => "danger",
                    _ => "light"
                },
                CreatedBy = t.CreatedBy?.FullName ?? "غير معروف",
                CreatedByEmail = t.CreatedBy?.Email ?? "",
                // ✅ هنا التعديل: نستخدم UserType من SupportTicket مش من ApplicationUser
                UserType = t.UserType == TicketType.Student ? "طالب" : "مدرس",
                CreatedAt = t.CreatedAt,
                RepliesCount = t.Replies?.Count ?? 0
            }).ToList();

            var stats = await _ticketService.GetTicketsStatisticsAsync();

            ViewBag.OpenCount = stats.GetValueOrDefault(TicketStatus.Open, 0);
            ViewBag.InProgressCount = stats.GetValueOrDefault(TicketStatus.InProgress, 0);
            ViewBag.ResolvedCount = stats.GetValueOrDefault(TicketStatus.Resolved, 0);
            ViewBag.ClosedCount = stats.GetValueOrDefault(TicketStatus.Closed, 0);

            return View(viewModel);
        }

        // GET: /Admin/TicketManagement/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var ticket = await _ticketService.GetTicketForAdminAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            var viewModel = new TicketDetailsViewModel
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Status = ticket.Status switch
                {
                    TicketStatus.Open => "مفتوحة",
                    TicketStatus.InProgress => "قيد المراجعة",
                    TicketStatus.Resolved => "تم الحل",
                    TicketStatus.Closed => "مغلقة",
                    _ => "غير معروف"
                },
                StatusColor = ticket.Status switch
                {
                    TicketStatus.Open => "warning",
                    TicketStatus.InProgress => "info",
                    TicketStatus.Resolved => "success",
                    TicketStatus.Closed => "secondary",
                    _ => "light"
                },
                Priority = ticket.Priority switch
                {
                    TicketPriority.Low => "منخفضة",
                    TicketPriority.Medium => "متوسطة",
                    TicketPriority.High => "عالية",
                    TicketPriority.Urgent => "عاجلة",
                    _ => "غير معروف"
                },
                PriorityColor = ticket.Priority switch
                {
                    TicketPriority.Low => "success",
                    TicketPriority.Medium => "primary",
                    TicketPriority.High => "warning",
                    TicketPriority.Urgent => "danger",
                    _ => "light"
                },
                CreatedByName = ticket.CreatedBy?.FullName ?? "غير معروف",
                CreatedByEmail = ticket.CreatedBy?.Email ?? "",
                // ✅ هنا التعديل: نستخدم UserType من SupportTicket
                UserType = ticket.UserType == TicketType.Student ? "طالب" : "مدرس",
                CreatedAt = ticket.CreatedAt,
                ClosedAt = ticket.ClosedAt,
                AttachmentPath = ticket.AttachmentPath,
                Replies = ticket.Replies?.Select(r => new TicketDetailsViewModel.TicketReplyViewModel
                {
                    Id = r.Id,
                    Message = r.Message,
                    UserName = r.User?.FullName ?? "غير معروف",
                    // ✅ هنا كمان تعديل: نعرف نوع المستخدم من خلال الـ Role
                    UserRole = GetUserRole(r.User),
                    CreatedAt = r.CreatedAt,
                    AttachmentPath = r.AttachmentPath
                }).ToList() ?? new()
            };

            return View(viewModel);
        }

        // POST: /Admin/TicketManagement/AddReply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReply(int id, AddReplyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _ticketService.AddReplyAsync(id, model.Message, userId, model.Attachment);

                TempData["Success"] = "تم إضافة الرد بنجاح";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إضافة رد");
                TempData["Error"] = "حدث خطأ أثناء إضافة الرد";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Admin/TicketManagement/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, TicketStatus status)
        {
            try
            {
                var updated = await _ticketService.UpdateTicketStatusAsync(id, status);

                if (updated)
                {
                    TempData["Success"] = "تم تحديث حالة التذكرة بنجاح";
                }
                else
                {
                    TempData["Error"] = "فشل في تحديث حالة التذكرة";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تحديث حالة تذكرة");
                TempData["Error"] = "حدث خطأ أثناء تحديث الحالة";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // ✅ دالة مساعدة لتحديد دور المستخدم
        private string GetUserRole(ApplicationUser? user)
        {
            if (user == null) return "غير معروف";

            // هنا المفروض تتحقق من الـ Roles بتاعت المستخدم
            // مؤقتاً هنرجع "أدمن" لو هو الأدمن الحالي
            if (User.IsInRole("Admin")) return "أدمن";
            if (User.IsInRole("Instructor")) return "مدرس";
            if (User.IsInRole("Student")) return "طالب";

            return "غير معروف";
        }
    }
}