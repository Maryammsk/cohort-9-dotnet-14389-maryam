using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Interfaces;
using TaskManagement.Identity;

namespace TaskManagement.API.Controllers;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/users")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public AdminUsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> GetUsers()
        => Ok(await _userManagementService.GetUsersAsync());

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
    {
        try
        {
            var user = await _userManagementService.CreateUserAsync(dto);
            return Created($"api/admin/users/{user.Id}", user);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{userId}/role")]
    public async Task<ActionResult<UserDto>> UpdateRole(string userId, UpdateUserRoleDto dto)
    {
        try
        {
            return Ok(await _userManagementService.UpdateRoleAsync(userId, dto.Role));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPatch("{userId}/access")]
    public async Task<ActionResult<UserDto>> UpdateAccess(string userId, UpdateUserAccessDto dto)
    {
        try
        {
            return Ok(await _userManagementService.UpdateAccessAsync(userId, dto.IsActive));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
