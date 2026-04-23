using SmartJob.API.DTOs.Chats;

namespace SmartJob.API.Services;

public interface IChatService
{
    Task<List<ChatDto>> GetMyChatsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ChatDto> GetOrCreateChatAsync(Guid userId, CreateChatRequest request, CancellationToken cancellationToken = default);
    Task<PagedMessages> GetMessagesAsync(Guid chatId, Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<MessageDto> SendMessageAsync(Guid chatId, Guid senderId, SendMessageRequest request, CancellationToken cancellationToken = default);
    Task MarkChatAsReadAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default);
}
