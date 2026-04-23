using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartJob.API.Data;
using SmartJob.API.DTOs.Chats;
using SmartJob.API.Exceptions;
using SmartJob.API.Models;

namespace SmartJob.API.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public ChatService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<ChatDto>> GetMyChatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var chats = await _db.Chats
            .Include(c => c.Seeker)
            .Include(c => c.Employer).ThenInclude(u => u.EmployerProfile)
            .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
            .Where(c => c.SeekerId == userId || c.EmployerId == userId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<ChatDto>>(chats);
    }

    public async Task<ChatDto> GetOrCreateChatAsync(Guid userId, CreateChatRequest request, CancellationToken cancellationToken = default)
    {
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == request.JobId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Job not found.");

        Guid seekerId, employerId;
        
        if (job.EmployerId == userId)
        {
            employerId = userId;
            seekerId = request.ParticipantId;
        }
        else
        {
            seekerId = userId;
            employerId = job.EmployerId;
        }

        var chat = await _db.Chats
            .Include(c => c.Seeker)
            .Include(c => c.Employer).ThenInclude(u => u.EmployerProfile)
            .FirstOrDefaultAsync(c => c.JobId == request.JobId && c.SeekerId == seekerId && c.EmployerId == employerId, cancellationToken);

        if (chat == null)
        {
            chat = new Chat
            {
                JobId = request.JobId,
                SeekerId = seekerId,
                EmployerId = employerId,
                JobTitle = job.Title,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };
            _db.Chats.Add(chat);
            await _db.SaveChangesAsync(cancellationToken);
            
            chat = await _db.Chats
                .Include(c => c.Seeker)
                .Include(c => c.Employer).ThenInclude(u => u.EmployerProfile)
                .FirstAsync(c => c.Id == chat.Id, cancellationToken);
        }

        return _mapper.Map<ChatDto>(chat);
    }

    public async Task<PagedMessages> GetMessagesAsync(Guid chatId, Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var chat = await _db.Chats.FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Chat not found.");

        if (chat.SeekerId != userId && chat.EmployerId != userId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Not authorized to view this chat.");

        var query = _db.Messages
            .Include(m => m.Sender)
            .Where(m => m.ChatId == chatId)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var messages = await query
            .OrderByDescending(m => m.SentAt) 
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedMessages
        {
            Items = _mapper.Map<List<MessageDto>>(messages),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<MessageDto> SendMessageAsync(Guid chatId, Guid senderId, SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        var chat = await _db.Chats.FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Chat not found.");

        if (chat.SeekerId != senderId && chat.EmployerId != senderId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Not authorized to send messages in this chat.");

        var message = new Message
        {
            ChatId = chatId,
            SenderId = senderId,
            Content = request.Content.Trim(),
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _db.Messages.Add(message);
        
        chat.LastMessageAt = message.SentAt;

        var receiverId = chat.SeekerId == senderId ? chat.EmployerId : chat.SeekerId;
        var notification = new Notification
        {
            UserId = receiverId,
            Type = NotificationType.Message,
            Title = "New Message",
            Message = $"You have a new message regarding {chat.JobTitle}.",
            CreatedAt = DateTime.UtcNow
        };
        _db.Notifications.Add(notification);

        await _db.SaveChangesAsync(cancellationToken);

        var sentMessage = await _db.Messages
            .Include(m => m.Sender)
            .FirstAsync(m => m.Id == message.Id, cancellationToken);
            
        return _mapper.Map<MessageDto>(sentMessage);
    }

    public async Task MarkChatAsReadAsync(Guid chatId, Guid userId, CancellationToken cancellationToken = default)
    {
        var chat = await _db.Chats.FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken)
            ?? throw new ApiException(StatusCodes.Status404NotFound, "Chat not found.");

        if (chat.SeekerId != userId && chat.EmployerId != userId)
            throw new ApiException(StatusCodes.Status403Forbidden, "Not authorized.");

        var unread = await _db.Messages
            .Where(m => m.ChatId == chatId && m.SenderId != userId && !m.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var msg in unread)
        {
            msg.IsRead = true;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
