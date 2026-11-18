namespace WorkItemApi.Models.Entities;

public class UserWorkItemStatus
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WorkItemId { get; set; }
    public bool IsChecked { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTimeOffset LastChangedAt { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }

    public AppUser? User { get; set; }
    public WorkItem? WorkItem { get; set; }
}
