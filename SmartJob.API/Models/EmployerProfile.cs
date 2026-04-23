using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartJob.API.Models;

public class EmployerProfile
{
    [Key]
    public Guid Id { get; set; }  // Same as User.Id (1-to-1)

    [Required, MaxLength(150)]
    public string CompanyName { get; set; } = string.Empty;

    public CompanySize CompanySize { get; set; } = CompanySize.Small;

    [MaxLength(100)]
    public string? Industry { get; set; }

    public string? CompanyLogoUrl { get; set; }

    [MaxLength(255)]
    public string? Website { get; set; }

    // Navigation
    [ForeignKey("Id")]
    public User User { get; set; } = null!;
}
