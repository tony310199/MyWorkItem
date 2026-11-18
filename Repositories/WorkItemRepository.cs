using Microsoft.EntityFrameworkCore;
using WorkItemApi.Data;
using WorkItemApi.Models.Entities;

namespace WorkItemApi.Repositories;

public class WorkItemRepository(AppDbContext db) : IWorkItemRepository
{
    public async Task<List<WorkItem>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.WorkItems.Where(x => !x.IsDeleted).AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public async Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.WorkItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

    public async Task AddAsync(WorkItem item, CancellationToken cancellationToken = default)
    {
        db.WorkItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WorkItem item, CancellationToken cancellationToken = default)
    {
        var existing = await db.WorkItems.FirstAsync(x => x.Id == item.Id && !x.IsDeleted, cancellationToken);
        existing.Title = item.Title;
        existing.Description = item.Description;
        existing.Category = item.Category;
        existing.Priority = item.Priority;
        existing.DueDate = item.DueDate;
        existing.UpdatedAt = item.UpdatedAt;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WorkItem item, CancellationToken cancellationToken = default)
    {
        var existing = await db.WorkItems.FirstAsync(x => x.Id == item.Id && !x.IsDeleted, cancellationToken);
        existing.IsDeleted = true;
        existing.UpdatedAt = item.UpdatedAt;
        await db.SaveChangesAsync(cancellationToken);
    }
}
