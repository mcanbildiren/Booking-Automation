using WhatsAppBookingService.Models;

namespace WhatsAppBookingService.Services
{
    public interface IConversationService
    {
        Task<ConversationState?> GetStateAsync(string phoneNumber);
        Task UpdateStateAsync(ConversationState state);
        Task ClearStateAsync(string phoneNumber);
    }
}

