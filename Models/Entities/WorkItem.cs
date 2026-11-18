namespace WorkItemApi.Models.Entities;

public class WorkItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public WorkItemPriority Priority { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<UserWorkItemStatus> UserStatuses { get; set; } = [];
}

public enum WorkItemPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}
