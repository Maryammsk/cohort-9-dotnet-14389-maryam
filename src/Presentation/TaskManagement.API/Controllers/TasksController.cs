using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TaskManagement.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Features.Tasks.Commands;
using TaskManagement.Application.Features.Tasks.Queries;

namespace TaskManagement.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll()
    {
        var tasks = await _mediator.Send(new GetAllTasksQuery());
        return Ok(FilterForCurrentUser(tasks));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id)
    {
        var task = await _mediator.Send(new GetTaskByIdQuery(id));
        return task is null || !CanAccess(task) ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskDto task)
    {
        var createdTask = await _mediator.Send(new CreateTaskCommand(task));
        return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, [FromBody] UpdateTaskDto task)
    {
        if (id != task.Id)
        {
            return BadRequest("Task id in the route does not match the task id in the payload.");
        }

        var updatedTask = await _mediator.Send(new UpdateTaskCommand(task));
        return Ok(updatedTask);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteTaskCommand(id));
        return NoContent();
    }

    private IEnumerable<TaskDto> FilterForCurrentUser(IEnumerable<TaskDto> tasks)
    {
        if (User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Manager))
        {
            return tasks;
        }

        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var userId)
            ? tasks.Where(task => task.AssignedUserId == userId)
            : [];
    }

    private bool CanAccess(TaskDto task)
        => User.IsInRole(Roles.Admin)
           || User.IsInRole(Roles.Manager)
           || (Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub"), out var userId)
               && task.AssignedUserId == userId);
}
