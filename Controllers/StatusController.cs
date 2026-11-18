using Microsoft.AspNetCore.Mvc;
using WorkItemApi.Models.DTOs;
using WorkItemApi.Services;

namespace WorkItemApi.Controllers;

[ApiController]
[Route("api/users/{userId:guid}")]
public class StatusController(StatusService service) : ControllerBase
{
    [HttpGet("work-items")]
    public async Task<IActionResult> GetWorkItems(Guid userId, CancellationToken cancellationToken)
        => Ok(await service.GetUserWorkItemsAsync(userId, cancellationToken));

    [HttpPut("work-item-statuses/{workItemId:guid}")]
    public async Task<IActionResult> SaveStatus(Guid userId, Guid workItemId, [FromBody] UserWorkItemStatusRequest request, CancellationToken cancellationToken)
    {
        await service.SaveStatusAsync(userId, workItemId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("work-item-statuses/confirm")]
    public async Task<IActionResult> Confirm(Guid userId, [FromBody] ConfirmWorkItemsRequest request, CancellationToken cancellationToken)
        => Ok(await service.ConfirmAsync(userId, request, cancellationToken));
}
