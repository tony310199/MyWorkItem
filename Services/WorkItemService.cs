using WorkItemApi.Exceptions;
using WorkItemApi.Models.DTOs;
using WorkItemApi.Models.Entities;
using WorkItemApi.Repositories;

namespace WorkItemApi.Services;

public class WorkItemService(IWorkItemRepository workItems, IUserRepository users)
{
    public async Task<List<WorkItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await workItems.GetAllAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<WorkItemDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await workItems.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("找不到指定的 Work Item。");
        return Map(item);
    }

    public async Task<WorkItemDto> CreateAsync(Guid? requesterId, WorkItemRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAdminAsync(requesterId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var item = new WorkItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Category = request.Category?.Trim(),
            Priority = request.Priority,
            DueDate = request.DueDate,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await workItems.AddAsync(item, cancellationToken);
        return Map(item);
    }

    public async Task UpdateAsync(Guid id, Guid? requesterId, WorkItemRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureAdminAsync(requesterId, cancellationToken);
        var existing = await workItems.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("找不到指定的 Work Item。");

        existing.Title = request.Title.Trim();
        existing.Description = request.Description?.Trim();
        existing.Category = request.Category?.Trim();
        existing.Priority = request.Priority;
        existing.DueDate = request.DueDate;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        await workItems.UpdateAsync(existing, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, Guid? requesterId, CancellationToken cancellationToken = default)
    {
        await EnsureAdminAsync(requesterId, cancellationToken);
        var existing = await workItems.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("找不到指定的 Work Item。");

        existing.IsDeleted = true;
        existing.UpdatedAt = DateTimeOffset.UtcNow;
        await workItems.DeleteAsync(existing, cancellationToken);
    }

    private async Task EnsureAdminAsync(Guid? requesterId, CancellationToken cancellationToken)
    {
        if (!requesterId.HasValue)
            throw new ForbiddenException("必須提供 X-User-Id。");

        var requester = await users.GetByIdAsync(requesterId.Value, cancellationToken)
            ?? throw new ForbiddenException("X-User-Id 無效。");

        if (requester.Role != UserRole.Admin)
            throw new ForbiddenException("只有管理者可以執行此操作。");
    }

    private static WorkItemDto Map(WorkItem item) => new()
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        Category = item.Category,
        Priority = item.Priority,
        DueDate = item.DueDate,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt
    };
}
