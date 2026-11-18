using Microsoft.EntityFrameworkCore;
using WorkItemApi.Data;
using WorkItemApi.Models.Entities;

namespace WorkItemApi.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<List<AppUser>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await db.Users.AsNoTracking().OrderBy(x => x.LoginName).ToListAsync(cancellationToken);

    public async Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<AppUser?> GetByLoginNameAsync(string loginName, CancellationToken cancellationToken = default) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.LoginName == loginName, cancellationToken);
}
