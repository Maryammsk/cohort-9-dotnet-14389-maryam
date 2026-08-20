using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Identity.Services;

public sealed class IdentityUserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityUserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync()
    {
        var users = await _userManager.Users.OrderBy(user => user.Email).ToListAsync();
        var result = new List<UserDto>(users.Count);

        foreach (var user in users)
        {
            result.Add(await ToDtoAsync(user));
        }

        return result;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        var role = NormalizeRole(dto.Role);
        await EnsureRolesAsync();

        var user = new ApplicationUser
        {
            UserName = dto.Email.Trim(),
            Email = dto.Email.Trim(),
            FirstName = dto.FirstName?.Trim(),
            LastName = dto.LastName?.Trim(),
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        EnsureSucceeded(result);
        await EnsureUserRoleAsync(user, role);
        return await ToDtoAsync(user);
    }

    public async Task<UserDto> UpdateRoleAsync(string userId, string role)
    {
        var user = await FindUserAsync(userId);
        var normalizedRole = NormalizeRole(role);
        await EnsureRolesAsync();
        var currentRoles = await _userManager.GetRolesAsync(user);
        EnsureSucceeded(await _userManager.RemoveFromRolesAsync(user, currentRoles));
        await EnsureUserRoleAsync(user, normalizedRole);
        return await ToDtoAsync(user);
    }

    public async Task<UserDto> UpdateAccessAsync(string userId, bool isActive)
    {
        var user = await FindUserAsync(userId);
        if (!isActive && await IsLastActiveAdminAsync(user))
        {
            throw new InvalidOperationException("The last active administrator cannot be deactivated.");
        }

        user.IsActive = isActive;
        EnsureSucceeded(await _userManager.UpdateAsync(user));
        return await ToDtoAsync(user);
    }

    private async Task<UserDto> ToDtoAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = roles.FirstOrDefault() ?? Roles.Client,
            IsActive = user.IsActive
        };
    }

    private async Task<ApplicationUser> FindUserAsync(string userId)
        => await _userManager.FindByIdAsync(userId)
           ?? throw new KeyNotFoundException($"User '{userId}' was not found.");

    private async Task EnsureUserRoleAsync(ApplicationUser user, string role)
        => EnsureSucceeded(await _userManager.AddToRoleAsync(user, role));

    private async Task EnsureRolesAsync()
    {
        foreach (var role in Roles.All)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                EnsureSucceeded(await _roleManager.CreateAsync(new IdentityRole(role)));
            }
        }
    }

    private async Task<bool> IsLastActiveAdminAsync(ApplicationUser user)
    {
        if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            return false;
        }

        var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
        return admins.Count(admin => admin.IsActive) <= 1;
    }

    private static string NormalizeRole(string role)
    {
        var normalizedRole = Roles.All.FirstOrDefault(candidate =>
            string.Equals(candidate, role?.Trim(), StringComparison.OrdinalIgnoreCase));
        return normalizedRole ?? throw new ArgumentException("Role must be Admin, Client, or Manager.", nameof(role));
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }
    }
}
