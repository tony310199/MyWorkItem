using WorkItemApi.Models.Entities;

namespace WorkItemApi.Repositories;

public interface IUserRepository
{
    Task<List<AppUser>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppUser?> GetByLoginNameAsync(string loginName, CancellationToken cancellationToken = default);
}
