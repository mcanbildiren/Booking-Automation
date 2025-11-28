using WhatsAppBookingService.Models;

namespace WhatsAppBookingService.Services
{
    public interface IMessageHandler
    {
        Task HandleIncomingMessageAsync(string from, string messageText, string? senderName);
        Task HandleInteractiveReplyAsync(string from, string replyId, string replyTitle);
    }
}

