namespace WorkItemApi.Models.Entities;

public class AppUser
{
    public Guid Id { get; set; }
    public string LoginName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<UserWorkItemStatus> WorkItemStatuses { get; set; } = [];
}

public enum UserRole
{
    User = 0,
    Admin = 1
}
