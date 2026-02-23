using AIChatActivityTracker.Models;
using System.ComponentModel.DataAnnotations;

namespace AIChatActivityTracker.DTOs;

public class UpdateActivityRequest
{
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public ActivityStatus? Status { get; set; }
}
