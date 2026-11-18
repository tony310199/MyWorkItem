using Microsoft.EntityFrameworkCore;
using WorkItemApi.Data;
using WorkItemApi.Models.Entities;

namespace WorkItemApi.Repositories;

public class StatusRepository(AppDbContext db) : IStatusRepository
{
    public async Task<List<UserWorkItemStatus>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await db.UserWorkItemStatuses.AsNoTracking()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<UserWorkItemStatus?> GetAsync(Guid userId, Guid workItemId, CancellationToken cancellationToken = default) =>
        await db.UserWorkItemStatuses
            .FirstOrDefaultAsync(x => x.UserId == userId && x.WorkItemId == workItemId, cancellationToken);

    public async Task<List<WorkItem>> GetAvailableWorkItemsAsync(CancellationToken cancellationToken = default) =>
        await db.WorkItems.AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task SaveStatusAsync(UserWorkItemStatus status, CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(status.UserId, status.WorkItemId, cancellationToken);
        if (existing is null)
        {
            db.UserWorkItemStatuses.Add(status);
        }
        else
        {
            existing.IsChecked = status.IsChecked;
            existing.IsConfirmed = false;
            existing.LastChangedAt = status.LastChangedAt;
            existing.ConfirmedAt = null;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ConfirmAsync(Guid userId, IReadOnlyDictionary<Guid, bool> statuses, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var workItems = await db.WorkItems
            .Where(x => !x.IsDeleted && statuses.Keys.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (workItems.Count != statuses.Count)
            throw new InvalidOperationException("確認清單包含不存在或已刪除的 WorkItem。");

        var now = DateTimeOffset.UtcNow;
        var existingStatuses = await db.UserWorkItemStatuses
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var status in statuses)
        {
            var existing = existingStatuses.FirstOrDefault(x => x.WorkItemId == status.Key);
            if (existing is null)
            {
                db.UserWorkItemStatuses.Add(new UserWorkItemStatus
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    WorkItemId = status.Key,
                    IsChecked = status.Value,
                    IsConfirmed = true,
                    LastChangedAt = now,
                    ConfirmedAt = now
                });
            }
            else
            {
                existing.IsChecked = status.Value;
                existing.IsConfirmed = true;
                existing.LastChangedAt = now;
                existing.ConfirmedAt = now;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
