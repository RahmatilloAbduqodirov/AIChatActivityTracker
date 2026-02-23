using AIChatActivityTracker.Models;
using System.Collections.Concurrent;

namespace AIChatActivityTracker.Services;

public class ActivityService : IActivityService
{
    private readonly ConcurrentDictionary<Guid, Activity> _activities = new();

    public Activity Create(string title, string? description, DateTime scheduledAt)
    {
        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            ScheduledAt = scheduledAt,
            Status = ActivityStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _activities[activity.Id] = activity;
        return activity;
    }

    public Activity? GetById(Guid id)
    {
        _activities.TryGetValue(id, out var activity);
        return activity;
    }

    public IEnumerable<Activity> GetAll(
        ActivityStatus? status = null,
        DateTime? scheduledAfter = null,
        DateTime? scheduledBefore = null)
    {
        var query = _activities.Values.AsEnumerable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (scheduledAfter.HasValue)
            query = query.Where(a => a.ScheduledAt >= scheduledAfter.Value);

        if (scheduledBefore.HasValue)
            query = query.Where(a => a.ScheduledAt <= scheduledBefore.Value);

        return query.OrderBy(a => a.ScheduledAt).ToList();
    }

    public Activity? Update(Guid id, string? title, string? description, DateTime? scheduledAt, ActivityStatus? status)
    {
        if (!_activities.TryGetValue(id, out var activity))
            return null;

        if (title is not null) activity.Title = title;
        if (description is not null) activity.Description = description;
        if (scheduledAt.HasValue) activity.ScheduledAt = scheduledAt.Value;
        if (status.HasValue) activity.Status = status.Value;
        activity.UpdatedAt = DateTime.UtcNow;

        return activity;
    }

    public bool Delete(Guid id)
    {
        return _activities.TryRemove(id, out _);
    }
}
