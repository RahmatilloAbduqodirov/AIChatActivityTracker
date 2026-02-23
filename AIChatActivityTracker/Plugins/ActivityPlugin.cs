using AIChatActivityTracker.Models;
using AIChatActivityTracker.Services;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace AIChatActivityTracker.Plugins;

public class ActivityPlugin
{
    private readonly IActivityService _activityService;

    public ActivityPlugin(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [KernelFunction("CreateActivity")]
    [Description("Creates a new activity with a title, optional description, and scheduled date/time. Returns the created activity details.")]
    public string CreateActivity(
        [Description("The title of the activity")] string title,
        [Description("An optional description of the activity")] string? description,
        [Description("The scheduled date and time for the activity in ISO 8601 format (e.g., 2025-12-25T10:00:00)")] DateTime scheduledAt)
    {
        var activity = _activityService.Create(title, description, scheduledAt);
        return $"Activity created successfully!\n" +
               $"  ID: {activity.Id}\n" +
               $"  Title: {activity.Title}\n" +
               $"  Description: {activity.Description ?? "None"}\n" +
               $"  Scheduled: {activity.ScheduledAt:g}\n" +
               $"  Status: {activity.Status}";
    }

    [KernelFunction("GetActivities")]
    [Description("Retrieves a list of all activities. Optionally filter by status (Pending, InProgress, Completed, Cancelled).")]
    public string GetActivities(
        [Description("Optional status filter: Pending, InProgress, Completed, or Cancelled")] string? status = null)
    {
        ActivityStatus? statusFilter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ActivityStatus>(status, true, out var parsed))
            statusFilter = parsed;

        var activities = _activityService.GetAll(statusFilter).ToList();

        if (activities.Count == 0)
            return "No activities found.";

        var result = $"Found {activities.Count} activity(ies):\n";
        foreach (var activity in activities)
        {
            result += $"\n  - [{activity.Status}] {activity.Title} (ID: {activity.Id})\n" +
                      $"    Scheduled: {activity.ScheduledAt:g}\n" +
                      $"    Description: {activity.Description ?? "None"}\n";
        }

        return result;
    }

    [KernelFunction("GetActivityById")]
    [Description("Retrieves a specific activity by its unique ID.")]
    public string GetActivityById(
        [Description("The unique identifier (GUID) of the activity")] string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return "Invalid activity ID format. Please provide a valid GUID.";

        var activity = _activityService.GetById(guid);
        if (activity is null)
            return $"No activity found with ID: {id}";

        return $"Activity details:\n" +
               $"  ID: {activity.Id}\n" +
               $"  Title: {activity.Title}\n" +
               $"  Description: {activity.Description ?? "None"}\n" +
               $"  Scheduled: {activity.ScheduledAt:g}\n" +
               $"  Status: {activity.Status}\n" +
               $"  Created: {activity.CreatedAt:g}\n" +
               $"  Updated: {activity.UpdatedAt?.ToString("g") ?? "Never"}";
    }

    [KernelFunction("UpdateActivity")]
    [Description("Updates an existing activity. You can change the title, description, scheduled time, or status. Only provide the fields you want to change.")]
    public string UpdateActivity(
        [Description("The unique identifier (GUID) of the activity to update")] string id,
        [Description("The new title, or null to keep unchanged")] string? title = null,
        [Description("The new description, or null to keep unchanged")] string? description = null,
        [Description("The new scheduled date/time in ISO 8601 format, or null to keep unchanged")] DateTime? scheduledAt = null,
        [Description("The new status: Pending, InProgress, Completed, or Cancelled, or null to keep unchanged")] string? status = null)
    {
        if (!Guid.TryParse(id, out var guid))
            return "Invalid activity ID format. Please provide a valid GUID.";

        ActivityStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status))
        {
            if (!Enum.TryParse<ActivityStatus>(status, true, out var parsed))
                return $"Invalid status: '{status}'. Valid values are: Pending, InProgress, Completed, Cancelled.";
            statusEnum = parsed;
        }

        var activity = _activityService.Update(guid, title, description, scheduledAt, statusEnum);
        if (activity is null)
            return $"No activity found with ID: {id}";

        return $"Activity updated successfully!\n" +
               $"  ID: {activity.Id}\n" +
               $"  Title: {activity.Title}\n" +
               $"  Description: {activity.Description ?? "None"}\n" +
               $"  Scheduled: {activity.ScheduledAt:g}\n" +
               $"  Status: {activity.Status}";
    }

    [KernelFunction("DeleteActivity")]
    [Description("Deletes an activity by its unique ID. This action cannot be undone.")]
    public string DeleteActivity(
        [Description("The unique identifier (GUID) of the activity to delete")] string id)
    {
        if (!Guid.TryParse(id, out var guid))
            return "Invalid activity ID format. Please provide a valid GUID.";

        var deleted = _activityService.Delete(guid);
        return deleted
            ? $"Activity with ID {id} has been deleted successfully."
            : $"No activity found with ID: {id}";
    }
}
