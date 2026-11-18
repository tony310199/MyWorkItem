using System.ComponentModel.DataAnnotations;
using WorkItemApi.Models.Entities;

namespace WorkItemApi.Models.DTOs;

public class WorkItemRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    public WorkItemPriority Priority { get; set; } = WorkItemPriority.Medium;
    public DateTimeOffset? DueDate { get; set; }
}

public class WorkItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public WorkItemPriority Priority { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class UserWorkItemDto : WorkItemDto
{
    public bool IsChecked { get; set; }
    public bool IsConfirmed { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
}

public class UserWorkItemStatusRequest
{
    [Required]
    public bool? IsChecked { get; set; }
}

public class ConfirmWorkItemsRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "至少需要一筆確認項目。")]
    public List<ConfirmStatusItem> Statuses { get; set; } = [];
}

public class ConfirmStatusItem
{
    [Required]
    public Guid? WorkItemId { get; set; }

    [Required]
    public bool? IsChecked { get; set; }
}

public class ConfirmResultDto
{
    public DateTimeOffset ConfirmedAt { get; set; }
    public int ConfirmedCount { get; set; }
}
