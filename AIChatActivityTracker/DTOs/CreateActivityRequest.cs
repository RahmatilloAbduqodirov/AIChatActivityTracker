using System.ComponentModel.DataAnnotations;

namespace AIChatActivityTracker.DTOs;

public class CreateActivityRequest
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }
}
