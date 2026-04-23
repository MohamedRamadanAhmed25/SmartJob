using System.ComponentModel.DataAnnotations;

namespace SmartJob.API.Models;

public class Chat
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid JobId { get; set; }
    public Guid SeekerId { get; set; }
    public Guid EmployerId { get; set; }

    // Denormalized for sticky header in UI
    [MaxLength(150)]
    public string JobTitle { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastMessageAt { get; set; }

    // Navigation
    public Job Job { get; set; } = null!;
    public User Seeker { get; set; } = null!;
    public User Employer { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
