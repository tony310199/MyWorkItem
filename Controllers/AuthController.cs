using Microsoft.AspNetCore.Mvc;
using WorkItemApi.Models.DTOs;
using WorkItemApi.Services;

namespace WorkItemApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService service) : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        => Ok(await service.GetUsersAsync(cancellationToken));

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        => Ok(await service.LoginAsync(request, cancellationToken));
}
