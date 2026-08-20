using TaskManagement.Application.Dtos;

namespace TaskManagement.Application.Interfaces;

public interface IUserManagementService
{
    Task<IReadOnlyCollection<UserDto>> GetUsersAsync();
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task<UserDto> UpdateRoleAsync(string userId, string role);
    Task<UserDto> UpdateAccessAsync(string userId, bool isActive);
}
