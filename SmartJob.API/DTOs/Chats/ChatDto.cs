namespace SmartJob.API.DTOs.Chats;

public class ChatDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public Guid SeekerId { get; set; }
    public string SeekerName { get; set; } = string.Empty;
    public string? SeekerAvatarUrl { get; set; }
    public Guid EmployerId { get; set; }
    public string EmployerName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderAvatarUrl { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}

public class CreateChatRequest
{
    public Guid JobId { get; set; }
    public Guid ParticipantId { get; set; }
}

public class SendMessageRequest
{
    public string Content { get; set; } = string.Empty;
}

public class PagedMessages
{
    public List<MessageDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
