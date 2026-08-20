using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Exceptions;
using TaskManagement.Application.Interfaces;
using TaskManagement.Infrastructure.Data;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TaskRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskDto?> GetByIdAsync(Guid id)
    {
        var task = await _dbContext.TaskItems
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id);

        return task is null ? null : ToDto(task);
    }

    public async Task<IEnumerable<TaskDto>> GetAllAsync()
    {
        var tasks = await _dbContext.TaskItems
            .AsNoTracking()
            .ToListAsync();

        return tasks.Select(ToDto);
    }

    public async Task<TaskDto> AddAsync(CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = dto.Status,
            Category = dto.Category,
            DueDate = dto.DueDate,
            AssignedUserId = dto.AssignedUserId,
            CreatedByUserId = dto.CreatedByUserId
        };

        _dbContext.TaskItems.Add(task);
        await _dbContext.SaveChangesAsync();

        return ToDto(task);
    }

    public async Task<TaskDto> UpdateAsync(UpdateTaskDto dto)
    {
        var task = await _dbContext.TaskItems.FirstOrDefaultAsync(item => item.Id == dto.Id)
            ?? throw new NotFoundException($"Task with id '{dto.Id}' was not found.");

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Priority = dto.Priority;
        task.Status = dto.Status;
        task.Category = dto.Category;
        task.DueDate = dto.DueDate;
        task.AssignedUserId = dto.AssignedUserId;
        task.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return ToDto(task);
    }

    public async Task DeleteAsync(Guid id)
    {
        var task = await _dbContext.TaskItems.FirstOrDefaultAsync(item => item.Id == id)
            ?? throw new NotFoundException($"Task with id '{id}' was not found.");

        _dbContext.TaskItems.Remove(task);
        await _dbContext.SaveChangesAsync();
    }

    private static TaskDto ToDto(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Priority = task.Priority,
        Status = task.Status,
        DueDate = task.DueDate,
        AssignedUserId = task.AssignedUserId,
        CreatedByUserId = task.CreatedByUserId,
        CreatedAt = task.CreatedAt
    };
}
