using AIChatActivityTracker.Models;

namespace AIChatActivityTracker.Services;

public interface IActivityService
{
    Activity Create(string title, string? description, DateTime scheduledAt);
    Activity? GetById(Guid id);
    IEnumerable<Activity> GetAll(
        ActivityStatus? status = null,
        DateTime? scheduledAfter = null,
        DateTime? scheduledBefore = null);
    Activity? Update(Guid id, string? title, string? description, DateTime? scheduledAt, ActivityStatus? status);
    bool Delete(Guid id);
}
