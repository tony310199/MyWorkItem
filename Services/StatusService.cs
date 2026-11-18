using WorkItemApi.Exceptions;
using WorkItemApi.Models.DTOs;
using WorkItemApi.Models.Entities;
using WorkItemApi.Repositories;

namespace WorkItemApi.Services;

public class StatusService(IStatusRepository statuses, IUserRepository users)
{
    public async Task<List<UserWorkItemDto>> GetUserWorkItemsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureUserAsync(userId, cancellationToken);
        var items = await statuses.GetAvailableWorkItemsAsync(cancellationToken);
        var userStatuses = await statuses.GetByUserAsync(userId, cancellationToken);

        return items.Select(item =>
        {
            var status = userStatuses.FirstOrDefault(x => x.WorkItemId == item.Id);
            return new UserWorkItemDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Category = item.Category,
                Priority = item.Priority,
                DueDate = item.DueDate,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt,
                IsChecked = status?.IsChecked ?? false,
                IsConfirmed = status?.IsConfirmed ?? false,
                ConfirmedAt = status?.ConfirmedAt
            };
        }).ToList();
    }

    public async Task SaveStatusAsync(Guid userId, Guid workItemId, UserWorkItemStatusRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureUserAsync(userId, cancellationToken);
        var workItems = await statuses.GetAvailableWorkItemsAsync(cancellationToken);
        if (workItems.All(x => x.Id != workItemId))
            throw new NotFoundException("找不到指定或已刪除的 Work Item。");

        await statuses.SaveStatusAsync(new UserWorkItemStatus
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            WorkItemId = workItemId,
            IsChecked = request.IsChecked!.Value,
            IsConfirmed = false,
            LastChangedAt = DateTimeOffset.UtcNow,
            ConfirmedAt = null
        }, cancellationToken);
    }

    public async Task<ConfirmResultDto> ConfirmAsync(Guid userId, ConfirmWorkItemsRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureUserAsync(userId, cancellationToken);

        var statusesDictionary = new Dictionary<Guid, bool>();
        foreach (var status in request.Statuses)
        {
            if (!statusesDictionary.TryAdd(status.WorkItemId!.Value, status.IsChecked!.Value))
                throw new ConflictException("Confirm 清單中包含重複的 WorkItemId。");
        }

        await statuses.ConfirmAsync(userId, statusesDictionary, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        return new ConfirmResultDto
        {
            ConfirmedAt = now,
            ConfirmedCount = statusesDictionary.Count
        };
    }

    private async Task EnsureUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (await users.GetByIdAsync(userId, cancellationToken) is null)
            throw new NotFoundException("找不到指定的使用者。");
    }
}
