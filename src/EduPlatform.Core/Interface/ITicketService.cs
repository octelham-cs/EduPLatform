using EduPlatform.Core.Entities;
using EduPlatform.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace EduPlatform.Core.Interfaces
{
    public interface ITicketService
    {
        // للطالب والمدرس
        Task<SupportTicket> CreateTicketAsync(string title, string description, string userId, TicketType userType, IFormFile? attachment = null);
        Task<List<SupportTicket>> GetUserTicketsAsync(string userId);
        Task<SupportTicket?> GetTicketByIdAsync(int ticketId, string userId);
        Task<TicketReply> AddReplyAsync(int ticketId, string message, string userId, IFormFile? attachment = null);
        Task<bool> CloseTicketAsync(int ticketId, string userId);

        // للأدمن
        Task<List<SupportTicket>> GetAllTicketsAsync(TicketStatus? status = null, TicketPriority? priority = null);
        Task<SupportTicket?> GetTicketForAdminAsync(int ticketId);
        Task<bool> AssignTicketAsync(int ticketId, int adminId);
        Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus);
        Task<List<SupportTicket>> GetTicketsByStatusAsync(TicketStatus status);
        Task<Dictionary<TicketStatus, int>> GetTicketsStatisticsAsync();
    }
}