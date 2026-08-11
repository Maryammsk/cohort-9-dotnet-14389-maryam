using System;
using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Dtos;

public class UpdateTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskStatus Status { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public Guid AssignedUserId { get; set; }
}
