using BookingAPI.Models;

namespace BookingAPI.Services
{
    public interface IConversationService
    {
        Task<ConversationState?> GetStateAsync(string phoneNumber);
        Task UpdateStateAsync(ConversationState state);
        Task ClearStateAsync(string phoneNumber);
    }
}

