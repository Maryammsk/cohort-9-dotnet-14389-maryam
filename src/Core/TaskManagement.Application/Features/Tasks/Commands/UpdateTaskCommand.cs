using MediatR;
using TaskManagement.Application.Dtos;

namespace TaskManagement.Application.Features.Tasks.Commands;

public record UpdateTaskCommand(UpdateTaskDto Task) : IRequest<TaskDto>;
