using AIChatActivityTracker.Models;
using AIChatActivityTracker.Plugins;
using AIChatActivityTracker.Services;
using Moq;

namespace AIChatActivityTracker.Tests;

public class ActivityPluginTests
{
    private readonly Mock<IActivityService> _serviceMock = new();
    private readonly ActivityPlugin _sut;

    public ActivityPluginTests()
    {
        _sut = new ActivityPlugin(_serviceMock.Object);
    }

    private static Activity MakeActivity(
        Guid? id = null,
        string title = "Test",
        string? description = null,
        ActivityStatus status = ActivityStatus.Pending) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Title = title,
        Description = description,
        ScheduledAt = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc),
        Status = status,
        CreatedAt = DateTime.UtcNow
    };

    // --- CreateActivity ---

    [Fact]
    public void CreateActivity_ReturnsFormattedString()
    {
        var activity = MakeActivity(title: "Meeting", description: "Weekly sync");
        _serviceMock
            .Setup(s => s.Create("Meeting", "Weekly sync", It.IsAny<DateTime>()))
            .Returns(activity);

        var result = _sut.CreateActivity("Meeting", "Weekly sync", activity.ScheduledAt);

        Assert.Contains("created successfully", result);
        Assert.Contains("Meeting", result);
        Assert.Contains(activity.Id.ToString(), result);
    }

    // --- GetActivities ---

    [Fact]
    public void GetActivities_NoActivities_ReturnsNotFoundMessage()
    {
        _serviceMock
            .Setup(s => s.GetAll(null, null, null))
            .Returns([]);

        var result = _sut.GetActivities();

        Assert.Equal("No activities found.", result);
    }

    [Fact]
    public void GetActivities_WithActivities_ReturnsFormattedList()
    {
        var activities = new List<Activity>
        {
            MakeActivity(title: "Task A"),
            MakeActivity(title: "Task B")
        };
        _serviceMock
            .Setup(s => s.GetAll(null, null, null))
            .Returns(activities);

        var result = _sut.GetActivities();

        Assert.Contains("Found 2 activity(ies)", result);
        Assert.Contains("Task A", result);
        Assert.Contains("Task B", result);
    }

    [Fact]
    public void GetActivities_WithValidStatusFilter_PassesParsedEnum()
    {
        _serviceMock
            .Setup(s => s.GetAll(ActivityStatus.Completed, null, null))
            .Returns([]);

        _sut.GetActivities("Completed");

        _serviceMock.Verify(s => s.GetAll(ActivityStatus.Completed, null, null), Times.Once);
    }

    [Fact]
    public void GetActivities_WithInvalidStatus_PassesNullFilter()
    {
        _serviceMock
            .Setup(s => s.GetAll(null, null, null))
            .Returns([]);

        _sut.GetActivities("InvalidStatus");

        _serviceMock.Verify(s => s.GetAll(null, null, null), Times.Once);
    }

    // --- GetActivityById ---

    [Fact]
    public void GetActivityById_ValidGuidAndFound_ReturnsDetails()
    {
        var activity = MakeActivity(title: "Found Task", description: "Details here");
        _serviceMock
            .Setup(s => s.GetById(activity.Id))
            .Returns(activity);

        var result = _sut.GetActivityById(activity.Id.ToString());

        Assert.Contains("Found Task", result);
        Assert.Contains("Details here", result);
        Assert.Contains(activity.Id.ToString(), result);
    }

    [Fact]
    public void GetActivityById_ValidGuidNotFound_ReturnsNotFoundMessage()
    {
        var id = Guid.NewGuid();
        _serviceMock
            .Setup(s => s.GetById(id))
            .Returns((Activity?)null);

        var result = _sut.GetActivityById(id.ToString());

        Assert.Contains("No activity found", result);
    }

    [Fact]
    public void GetActivityById_InvalidGuid_ReturnsErrorMessage()
    {
        var result = _sut.GetActivityById("not-a-guid");

        Assert.Contains("Invalid activity ID", result);
    }

    // --- UpdateActivity ---

    [Fact]
    public void UpdateActivity_ValidUpdate_ReturnsSuccessMessage()
    {
        var activity = MakeActivity(title: "Updated Task", status: ActivityStatus.InProgress);
        _serviceMock
            .Setup(s => s.Update(activity.Id, "Updated Task", null, null, ActivityStatus.InProgress))
            .Returns(activity);

        var result = _sut.UpdateActivity(activity.Id.ToString(), "Updated Task", status: "InProgress");

        Assert.Contains("updated successfully", result);
        Assert.Contains("Updated Task", result);
    }

    [Fact]
    public void UpdateActivity_InvalidGuid_ReturnsErrorMessage()
    {
        var result = _sut.UpdateActivity("bad-guid", "Title");

        Assert.Contains("Invalid activity ID", result);
    }

    [Fact]
    public void UpdateActivity_InvalidStatus_ReturnsErrorMessage()
    {
        var id = Guid.NewGuid();

        var result = _sut.UpdateActivity(id.ToString(), status: "BadStatus");

        Assert.Contains("Invalid status", result);
        Assert.Contains("BadStatus", result);
    }

    [Fact]
    public void UpdateActivity_NotFound_ReturnsNotFoundMessage()
    {
        var id = Guid.NewGuid();
        _serviceMock
            .Setup(s => s.Update(id, "Title", null, null, null))
            .Returns((Activity?)null);

        var result = _sut.UpdateActivity(id.ToString(), "Title");

        Assert.Contains("No activity found", result);
    }

    // --- DeleteActivity ---

    [Fact]
    public void DeleteActivity_Exists_ReturnsDeletedMessage()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.Delete(id)).Returns(true);

        var result = _sut.DeleteActivity(id.ToString());

        Assert.Contains("deleted successfully", result);
    }

    [Fact]
    public void DeleteActivity_NotFound_ReturnsNotFoundMessage()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.Delete(id)).Returns(false);

        var result = _sut.DeleteActivity(id.ToString());

        Assert.Contains("No activity found", result);
    }

    [Fact]
    public void DeleteActivity_InvalidGuid_ReturnsErrorMessage()
    {
        var result = _sut.DeleteActivity("invalid");

        Assert.Contains("Invalid activity ID", result);
    }
}
