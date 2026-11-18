using System.ComponentModel.DataAnnotations;
using WorkItemApi.Models.Entities;

namespace WorkItemApi.Models.DTOs;

public class LoginRequest
{
    [Required, StringLength(100)]
    public string LoginName { get; set; } = string.Empty;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string LoginName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
