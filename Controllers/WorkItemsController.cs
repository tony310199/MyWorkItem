using Microsoft.AspNetCore.Mvc;
using WorkItemApi.Models.DTOs;
using WorkItemApi.Services;

namespace WorkItemApi.Controllers;

[ApiController]
[Route("api/work-items")]
public class WorkItemsController(WorkItemService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WorkItemRequest request, CancellationToken cancellationToken)
    {
        var requesterId = GetRequesterId();
        var item = await service.CreateAsync(requesterId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] WorkItemRequest request, CancellationToken cancellationToken)
    {
        await service.UpdateAsync(id, GetRequesterId(), request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, GetRequesterId(), cancellationToken);
        return NoContent();
    }

    private Guid? GetRequesterId()
    {
        if (Request.Headers.TryGetValue("X-User-Id", out var value) && Guid.TryParse(value.ToString(), out var id))
            return id;
        return null;
    }
}
