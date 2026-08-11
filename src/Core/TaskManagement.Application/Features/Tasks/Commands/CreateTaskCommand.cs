using System;
using MediatR;
using TaskManagement.Application.Dtos;

namespace TaskManagement.Application.Features.Tasks.Commands;

public record CreateTaskCommand(CreateTaskDto Task) : IRequest<TaskDto>;
