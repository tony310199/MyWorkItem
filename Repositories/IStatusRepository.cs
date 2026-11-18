using WorkItemApi.Models.Entities;

namespace WorkItemApi.Repositories;

public interface IStatusRepository
{
    Task<List<UserWorkItemStatus>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserWorkItemStatus?> GetAsync(Guid userId, Guid workItemId, CancellationToken cancellationToken = default);
    Task<List<WorkItem>> GetAvailableWorkItemsAsync(CancellationToken cancellationToken = default);
    Task SaveStatusAsync(UserWorkItemStatus status, CancellationToken cancellationToken = default);
    Task ConfirmAsync(Guid userId, IReadOnlyDictionary<Guid, bool> statuses, CancellationToken cancellationToken = default);
}
