using WorkItemApi.Models.Entities;

namespace WorkItemApi.Repositories;

public interface IWorkItemRepository
{
    Task<List<WorkItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(WorkItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(WorkItem item, CancellationToken cancellationToken = default);
}
