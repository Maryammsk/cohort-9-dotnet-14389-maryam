using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Features.Tasks.Commands;
using TaskManagement.Application.Features.Tasks.Queries;
using TaskManagement.Application.Interfaces;

namespace TaskManagement.Application.Services;

public class TaskService :
    IRequestHandler<CreateTaskCommand, TaskDto>,
    IRequestHandler<GetTaskByIdQuery, TaskDto?>,
    IRequestHandler<GetAllTasksQuery, IEnumerable<TaskDto>>,
    IRequestHandler<UpdateTaskCommand, TaskDto>,
    IRequestHandler<DeleteTaskCommand, Unit>
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        return await _taskRepository.AddAsync(request.Task);
    }

    public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        return await _taskRepository.GetByIdAsync(request.Id);
    }

    public async Task<IEnumerable<TaskDto>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        return await _taskRepository.GetAllAsync();
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        return await _taskRepository.UpdateAsync(request.Task);
    }

    public async Task<Unit> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        await _taskRepository.DeleteAsync(request.Id);
        return Unit.Value;
    }
}
