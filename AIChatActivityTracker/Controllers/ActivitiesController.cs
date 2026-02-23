using AIChatActivityTracker.DTOs;
using AIChatActivityTracker.Models;
using AIChatActivityTracker.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIChatActivityTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ActivityResponse>> GetAll(
        [FromQuery] ActivityStatus? status,
        [FromQuery] DateTime? scheduledAfter,
        [FromQuery] DateTime? scheduledBefore)
    {
        var activities = _activityService.GetAll(status, scheduledAfter, scheduledBefore);
        return Ok(activities.Select(MapToResponse));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<ActivityResponse> GetById(Guid id)
    {
        var activity = _activityService.GetById(id);
        if (activity is null)
            return NotFound();

        return Ok(MapToResponse(activity));
    }

    [HttpPost]
    public ActionResult<ActivityResponse> Create([FromBody] CreateActivityRequest request)
    {
        var activity = _activityService.Create(
            request.Title,
            request.Description,
            request.ScheduledAt);

        return CreatedAtAction(
            nameof(GetById),
            new { id = activity.Id },
            MapToResponse(activity));
    }

    [HttpPut("{id:guid}")]
    public ActionResult<ActivityResponse> Update(Guid id, [FromBody] UpdateActivityRequest request)
    {
        var activity = _activityService.Update(
            id,
            request.Title,
            request.Description,
            request.ScheduledAt,
            request.Status);

        if (activity is null)
            return NotFound();

        return Ok(MapToResponse(activity));
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var deleted = _activityService.Delete(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private static ActivityResponse MapToResponse(Activity activity) => new()
    {
        Id = activity.Id,
        Title = activity.Title,
        Description = activity.Description,
        ScheduledAt = activity.ScheduledAt,
        Status = activity.Status.ToString(),
        CreatedAt = activity.CreatedAt,
        UpdatedAt = activity.UpdatedAt
    };
}
