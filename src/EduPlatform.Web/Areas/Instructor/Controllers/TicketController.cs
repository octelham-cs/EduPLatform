using EduPlatform.Core.Enums;
using EduPlatform.Core.Interfaces;
using EduPlatform.Web.ViewModels.Ticket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace EduPlatform.Web.Areas.Instructor.Controllers
{
    [Area("Instructor")]
    [Authorize(Roles = "Instructor")]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketController> _logger;

        public TicketController(ITicketService ticketService, ILogger<TicketController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        // GET: /Instructor/Ticket
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tickets = await _ticketService.GetUserTicketsAsync(userId);

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
                CreatedAt = t.CreatedAt,
                RepliesCount = t.Replies?.Count ?? 0
            }).ToList();

            return View(viewModel);
        }

        // GET: /Instructor/Ticket/Create
        public IActionResult Create()
        {
            var viewModel = new CreateTicketViewModel
            {
                Priorities = new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "منخفضة" },
                    new SelectListItem { Value = "2", Text = "متوسطة", Selected = true },
                    new SelectListItem { Value = "3", Text = "عالية" },
                    new SelectListItem { Value = "4", Text = "عاجلة" }
                }
            };
            return View(viewModel);
        }

        // POST: /Instructor/Ticket/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTicketViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Priorities = new List<SelectListItem>
                {
                    new SelectListItem { Value = "1", Text = "منخفضة" },
                    new SelectListItem { Value = "2", Text = "متوسطة", Selected = model.Priority == 2 },
                    new SelectListItem { Value = "3", Text = "عالية", Selected = model.Priority == 3 },
                    new SelectListItem { Value = "4", Text = "عاجلة", Selected = model.Priority == 4 }
                };
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var ticket = await _ticketService.CreateTicketAsync(
                    model.Title,
                    model.Description,
                    userId,
                    TicketType.Instructor,
                    model.Attachment
                );

                TempData["Success"] = "تم إنشاء التذكرة بنجاح";
                return RedirectToAction(nameof(Details), new { id = ticket.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إنشاء تذكرة");
                ModelState.AddModelError("", "حدث خطأ أثناء إنشاء التذكرة");
                return View(model);
            }
        }

        // GET: /Instructor/Ticket/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ticket = await _ticketService.GetTicketByIdAsync(id, userId);

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
                CreatedAt = ticket.CreatedAt,
                ClosedAt = ticket.ClosedAt,
                AttachmentPath = ticket.AttachmentPath,
                Replies = ticket.Replies?.Select(r => new TicketDetailsViewModel.TicketReplyViewModel
                {
                    Id = r.Id,
                    Message = r.Message,
                    UserName = r.User?.FullName ?? "غير معروف",
                    UserRole = "مدرس",
                    CreatedAt = r.CreatedAt,
                    AttachmentPath = r.AttachmentPath
                }).ToList() ?? new()
            };

            return View(viewModel);
        }

        // POST: /Instructor/Ticket/AddReply
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

        // POST: /Instructor/Ticket/Close/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Close(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var closed = await _ticketService.CloseTicketAsync(id, userId);

                if (closed)
                {
                    TempData["Success"] = "تم إغلاق التذكرة بنجاح";
                }
                else
                {
                    TempData["Error"] = "فشل في إغلاق التذكرة";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إغلاق تذكرة");
                TempData["Error"] = "حدث خطأ أثناء إغلاق التذكرة";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}