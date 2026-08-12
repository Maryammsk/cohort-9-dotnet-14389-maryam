using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MediatR;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Features.Tasks.Commands;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using Xunit;

namespace TaskManagement.Tests.Features.Tasks;

public class UpdateTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTaskIsUpdated_ReturnsUpdatedTask()
    {
        // Arrange
        var request = new UpdateTaskCommand(new UpdateTaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Updated title",
            Description = "Updated description",
            Priority = Domain.Enums.TaskPriority.Low,
            Status = Domain.Enums.TaskStatus.InProgress,
            Category = "Maintenance",
            DueDate = DateTime.UtcNow.AddDays(5),
            AssignedUserId = Guid.NewGuid()
        });

        var expectedTask = new TaskDto
        {
            Id = request.Task.Id,
            Title = request.Task.Title,
            Description = request.Task.Description,
            Priority = request.Task.Priority,
            Status = request.Task.Status,
            DueDate = request.Task.DueDate,
            CreatedAt = DateTime.UtcNow
        };

        var repositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.UpdateAsync(request.Task))
            .ReturnsAsync(expectedTask);

        var service = new TaskService(repositoryMock.Object);

        // Act
        var actual = await service.Handle(request, CancellationToken.None);

        // Assert
        actual.Should().BeEquivalentTo(expectedTask);
        repositoryMock.Verify(x => x.UpdateAsync(request.Task), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new UpdateTaskCommand(new UpdateTaskDto
        {
            Id = Guid.NewGuid(),
            Title = "Nonexistent task",
            AssignedUserId = Guid.NewGuid()
        });

        var repositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.UpdateAsync(request.Task))
            .ThrowsAsync(new KeyNotFoundException("Task not found."));

        var service = new TaskService(repositoryMock.Object);

        // Act
        Func<Task> act = async () => await service.Handle(request, CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Task not found.");

        repositoryMock.Verify(x => x.UpdateAsync(request.Task), Times.Once);
    }
}
