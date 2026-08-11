using System.Collections.Generic;
using MediatR;
using TaskManagement.Application.Dtos;

namespace TaskManagement.Application.Features.Tasks.Queries;

public record GetAllTasksQuery() : IRequest<IEnumerable<TaskDto>>;
