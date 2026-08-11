using System;
using TaskManagement.Domain.Enums;
using TaskStatus = TaskManagement.Domain.Enums.TaskStatus;

namespace TaskManagement.Application.Dtos;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
    public string Category { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public Guid AssignedUserId { get; set; }
    public Guid CreatedByUserId { get; set; }
}
