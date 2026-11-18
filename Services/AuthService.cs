using WorkItemApi.Exceptions;
using WorkItemApi.Models.DTOs;
using WorkItemApi.Repositories;

namespace WorkItemApi.Services;

public class AuthService(IUserRepository users)
{
    public async Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var entities = await users.GetAllAsync(cancellationToken);
        return entities.Select(x => new UserDto { Id = x.Id, LoginName = x.LoginName, UserName = x.UserName, Role = x.Role }).ToList();
    }

    public async Task<UserDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await users.GetByLoginNameAsync(request.LoginName.Trim(), cancellationToken)
            ?? throw new NotFoundException("找不到指定的登入帳號。");

        return new UserDto { Id = user.Id, LoginName = user.LoginName, UserName = user.UserName, Role = user.Role };
    }
}
