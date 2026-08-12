using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Features.Tasks.Queries;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using Xunit;

namespace TaskManagement.Tests.Features.Tasks;

public class GetTaskByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenTaskExists_ReturnsTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var request = new GetTaskByIdQuery(taskId);

        var expectedTask = new TaskDto
        {
            Id = taskId,
            Title = "Existing task",
            Description = "Task details",
            Priority = Domain.Enums.TaskPriority.Medium,
            Status = Domain.Enums.TaskStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(2),
            CreatedAt = DateTime.UtcNow
        };

        var repositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync(expectedTask);

        var service = new TaskService(repositoryMock.Object);

        // Act
        var actual = await service.Handle(request, CancellationToken.None);

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(expectedTask);
        repositoryMock.Verify(x => x.GetByIdAsync(taskId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ReturnsNull()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var request = new GetTaskByIdQuery(taskId);

        var repositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.GetByIdAsync(taskId))
            .ReturnsAsync((TaskDto?)null);

        var service = new TaskService(repositoryMock.Object);

        // Act
        var actual = await service.Handle(request, CancellationToken.None);

        // Assert
        actual.Should().BeNull();
        repositoryMock.Verify(x => x.GetByIdAsync(taskId), Times.Once);
    }
}
