using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Interfaces;
using TaskManagement.Identity;

namespace TaskManagement.Identity.Services;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
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
        await _userManager.AddToRoleAsync(user, Roles.Client);
        return await CreateResponseAsync(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        await EnsureRolesAsync();
        var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
        if (user is null || !user.IsActive || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!(await _userManager.GetRolesAsync(user)).Any())
        {
            EnsureSucceeded(await _userManager.AddToRoleAsync(user, Roles.Client));
        }

        return await CreateResponseAsync(user);
    }

    private async Task<AuthResponseDto> CreateResponseAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = roles.Select(role => new Claim(ClaimTypes.Role, role));
        return new AuthResponseDto
        {
            Token = _tokenGenerator.GenerateToken(user, claims),
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName
        };
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }
    }

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
}
