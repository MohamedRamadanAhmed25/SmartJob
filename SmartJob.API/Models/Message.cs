using System.ComponentModel.DataAnnotations;

namespace SmartJob.API.Models;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Chat Chat { get; set; } = null!;
    public User Sender { get; set; } = null!;
}
