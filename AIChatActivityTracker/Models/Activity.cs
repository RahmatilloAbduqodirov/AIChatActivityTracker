namespace AIChatActivityTracker.Models;

public class Activity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ScheduledAt { get; set; }
    public ActivityStatus Status { get; set; } = ActivityStatus.Pending;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
