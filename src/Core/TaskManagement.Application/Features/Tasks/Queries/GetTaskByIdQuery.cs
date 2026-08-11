using System;
using MediatR;
using TaskManagement.Application.Dtos;

namespace TaskManagement.Application.Features.Tasks.Queries;

public record GetTaskByIdQuery(Guid Id) : IRequest<TaskDto?>;
