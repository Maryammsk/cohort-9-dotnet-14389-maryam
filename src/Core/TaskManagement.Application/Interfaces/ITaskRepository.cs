using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManagement.Application.Dtos;

namespace TaskManagement.Application.Interfaces;

public interface ITaskRepository
{
    Task<TaskDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TaskDto>> GetAllAsync();
    Task<TaskDto> AddAsync(CreateTaskDto dto);
    Task<TaskDto> UpdateAsync(UpdateTaskDto dto);
    Task DeleteAsync(Guid id);
}
