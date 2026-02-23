using AIChatActivityTracker.Models;
using AIChatActivityTracker.Services;

namespace AIChatActivityTracker.Tests;

public class ActivityServiceTests
{
    private readonly ActivityService _sut = new();

    // --- Create ---

    [Fact]
    public void Create_SetsPropertiesCorrectly()
    {
        var scheduledAt = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);

        var activity = _sut.Create("Meeting", "Team sync", scheduledAt);

        Assert.NotEqual(Guid.Empty, activity.Id);
        Assert.Equal("Meeting", activity.Title);
        Assert.Equal("Team sync", activity.Description);
        Assert.Equal(scheduledAt, activity.ScheduledAt);
        Assert.Equal(ActivityStatus.Pending, activity.Status);
        Assert.True(activity.CreatedAt <= DateTime.UtcNow);
        Assert.Null(activity.UpdatedAt);
    }

    [Fact]
    public void Create_WithNullDescription_SetsDescriptionNull()
    {
        var activity = _sut.Create("Task", null, DateTime.UtcNow.AddDays(1));

        Assert.Null(activity.Description);
    }

    // --- GetById ---

    [Fact]
    public void GetById_ExistingActivity_ReturnsActivity()
    {
        var created = _sut.Create("Test", null, DateTime.UtcNow);

        var result = _sut.GetById(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test", result.Title);
    }

    [Fact]
    public void GetById_NonExistentId_ReturnsNull()
    {
        var result = _sut.GetById(Guid.NewGuid());

        Assert.Null(result);
    }

    // --- GetAll ---

    [Fact]
    public void GetAll_ReturnsAllActivities()
    {
        _sut.Create("A", null, DateTime.UtcNow);
        _sut.Create("B", null, DateTime.UtcNow);
        _sut.Create("C", null, DateTime.UtcNow);

        var result = _sut.GetAll().ToList();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetAll_FilterByStatus_ReturnsOnlyMatching()
    {
        var a1 = _sut.Create("Pending1", null, DateTime.UtcNow);
        var a2 = _sut.Create("Completed1", null, DateTime.UtcNow);
        _sut.Update(a2.Id, null, null, null, ActivityStatus.Completed);

        var result = _sut.GetAll(status: ActivityStatus.Completed).ToList();

        Assert.Single(result);
        Assert.Equal("Completed1", result[0].Title);
    }

    [Fact]
    public void GetAll_FilterByScheduledAfter_ReturnsOnlyOnOrAfter()
    {
        var cutoff = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        _sut.Create("Before", null, cutoff.AddDays(-1));
        _sut.Create("On", null, cutoff);
        _sut.Create("After", null, cutoff.AddDays(1));

        var result = _sut.GetAll(scheduledAfter: cutoff).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.True(a.ScheduledAt >= cutoff));
    }

    [Fact]
    public void GetAll_FilterByScheduledBefore_ReturnsOnlyOnOrBefore()
    {
        var cutoff = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        _sut.Create("Before", null, cutoff.AddDays(-1));
        _sut.Create("On", null, cutoff);
        _sut.Create("After", null, cutoff.AddDays(1));

        var result = _sut.GetAll(scheduledBefore: cutoff).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, a => Assert.True(a.ScheduledAt <= cutoff));
    }

    [Fact]
    public void GetAll_ReturnsOrderedByScheduledAt()
    {
        var baseDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        _sut.Create("Third", null, baseDate.AddDays(3));
        _sut.Create("First", null, baseDate.AddDays(1));
        _sut.Create("Second", null, baseDate.AddDays(2));

        var result = _sut.GetAll().ToList();

        Assert.Equal("First", result[0].Title);
        Assert.Equal("Second", result[1].Title);
        Assert.Equal("Third", result[2].Title);
    }

    [Fact]
    public void GetAll_NoMatches_ReturnsEmpty()
    {
        var result = _sut.GetAll(status: ActivityStatus.Completed).ToList();

        Assert.Empty(result);
    }

    // --- Update ---

    [Fact]
    public void Update_TitleOnly_UpdatesOnlyTitle()
    {
        var created = _sut.Create("Original", "Desc", DateTime.UtcNow);

        var updated = _sut.Update(created.Id, "New Title", null, null, null);

        Assert.NotNull(updated);
        Assert.Equal("New Title", updated.Title);
        Assert.Equal("Desc", updated.Description);
        Assert.Equal(ActivityStatus.Pending, updated.Status);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public void Update_StatusOnly_UpdatesOnlyStatus()
    {
        var created = _sut.Create("Task", null, DateTime.UtcNow);

        var updated = _sut.Update(created.Id, null, null, null, ActivityStatus.InProgress);

        Assert.NotNull(updated);
        Assert.Equal("Task", updated.Title);
        Assert.Equal(ActivityStatus.InProgress, updated.Status);
    }

    [Fact]
    public void Update_MultipleFields_UpdatesAllSpecified()
    {
        var originalDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var newDate = new DateTime(2026, 6, 15, 14, 0, 0, DateTimeKind.Utc);
        var created = _sut.Create("Old", "Old desc", originalDate);

        var updated = _sut.Update(created.Id, "New", "New desc", newDate, ActivityStatus.Completed);

        Assert.NotNull(updated);
        Assert.Equal("New", updated.Title);
        Assert.Equal("New desc", updated.Description);
        Assert.Equal(newDate, updated.ScheduledAt);
        Assert.Equal(ActivityStatus.Completed, updated.Status);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public void Update_NonExistentId_ReturnsNull()
    {
        var result = _sut.Update(Guid.NewGuid(), "Title", null, null, null);

        Assert.Null(result);
    }

    // --- Delete ---

    [Fact]
    public void Delete_ExistingActivity_ReturnsTrueAndRemoves()
    {
        var created = _sut.Create("ToDelete", null, DateTime.UtcNow);

        var deleted = _sut.Delete(created.Id);

        Assert.True(deleted);
        Assert.Null(_sut.GetById(created.Id));
    }

    [Fact]
    public void Delete_NonExistentId_ReturnsFalse()
    {
        var result = _sut.Delete(Guid.NewGuid());

        Assert.False(result);
    }
}
