using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using EduPlatform.Core.Interfaces;
using EduPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EduPlatform.Infrastructure.Services
{
    public class TicketService : ITicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TicketService> _logger;
        private readonly IHostingEnvironment _env;  // ✅ IHostingEnvironment (قديمة)

        public TicketService(
            ApplicationDbContext context,
            ILogger<TicketService> logger,
            IHostingEnvironment env)  // ✅ IHostingEnvironment
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        // للطالب والمدرس
        public async Task<SupportTicket> CreateTicketAsync(string title, string description, string userId, TicketType userType, IFormFile? attachment = null)
        {
            string? attachmentPath = null;

            if (attachment is not null)
            {
                attachmentPath = await SaveFileAsync(attachment, "tickets");
            }

            var ticket = new SupportTicket
            {
                Title = title,
                Description = description,
                CreatedById = userId,
                UserType = userType,
                Status = TicketStatus.Open,
                Priority = TicketPriority.Medium,
                CreatedAt = DateTime.UtcNow,
                AttachmentPath = attachmentPath
            };

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket created: {TicketId} by user: {UserId}", ticket.Id, userId);

            return ticket;
        }

        public async Task<List<SupportTicket>> GetUserTicketsAsync(string userId)
        {
            return await _context.SupportTickets
                .Include(t => t.Replies)
                .Where(t => t.CreatedById == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<SupportTicket?> GetTicketByIdAsync(int ticketId, string userId)
        {
            return await _context.SupportTickets
                .Include(t => t.CreatedBy)
                .Include(t => t.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.CreatedById == userId);
        }

        public async Task<TicketReply> AddReplyAsync(int ticketId, string message, string userId, IFormFile? attachment = null)
        {
            string? attachmentPath = null;

            if (attachment is not null)
            {
                attachmentPath = await SaveFileAsync(attachment, "replies");
            }

            var reply = new TicketReply
            {
                TicketId = ticketId,
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.UtcNow,
                AttachmentPath = attachmentPath
            };

            _context.TicketReplies.Add(reply);

            // تحديث حالة التذكرة
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket != null && ticket.Status == TicketStatus.Open)
            {
                ticket.Status = TicketStatus.InProgress;
                ticket.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return reply;
        }

        public async Task<bool> CloseTicketAsync(int ticketId, string userId)
        {
            var ticket = await _context.SupportTickets
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.CreatedById == userId);

            if (ticket == null)
                return false;

            ticket.Status = TicketStatus.Closed;
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // للأدمن
        public async Task<List<SupportTicket>> GetAllTicketsAsync(TicketStatus? status = null, TicketPriority? priority = null)
        {
            var query = _context.SupportTickets
                .Include(t => t.CreatedBy)
                .Include(t => t.Replies)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            return await query
                .OrderByDescending(t => t.Priority)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<SupportTicket?> GetTicketForAdminAsync(int ticketId)
        {
            return await _context.SupportTickets
                .Include(t => t.CreatedBy)
                .Include(t => t.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(t => t.Id == ticketId);
        }

        public async Task<bool> AssignTicketAsync(int ticketId, int adminId)
        {
            // مؤقتاً مش هنستخدمها
            return false;
        }

        public async Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus)
        {
            var ticket = await _context.SupportTickets.FindAsync(ticketId);
            if (ticket == null)
                return false;

            ticket.Status = newStatus;
            ticket.UpdatedAt = DateTime.UtcNow;

            if (newStatus == TicketStatus.Closed || newStatus == TicketStatus.Resolved)
            {
                ticket.ClosedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SupportTicket>> GetTicketsByStatusAsync(TicketStatus status)
        {
            return await _context.SupportTickets
                .Include(t => t.CreatedBy)
                .Where(t => t.Status == status)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Dictionary<TicketStatus, int>> GetTicketsStatisticsAsync()
        {
            var stats = await _context.SupportTickets
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.Status, g => g.Count);

            return stats;
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            try
            {
                // ✅ IHostingEnvironment تستخدم ContentRootPath
                var uploadsFolder = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return $"/uploads/{folder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file");
                return null;
            }
        }
    }
}