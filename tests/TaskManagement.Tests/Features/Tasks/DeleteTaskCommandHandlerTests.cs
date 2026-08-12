using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MediatR;
using TaskManagement.Application.Features.Tasks.Commands;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using Xunit;

namespace TaskManagement.Tests.Features.Tasks;

public class DeleteTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTaskIsDeleted_ReturnsUnitValue()
    {
        // Arrange
        var request = new DeleteTaskCommand(Guid.NewGuid());

        var repositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.DeleteAsync(request.Id))
            .Returns(Task.CompletedTask);

        var service = new TaskService(repositoryMock.Object);

        // Act
        var actual = await service.Handle(request, CancellationToken.None);

        // Assert
        actual.Should().Be(Unit.Value);
        repositoryMock.Verify(x => x.DeleteAsync(request.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTaskDoesNotExist_ThrowsKeyNotFoundException()
    {
        // Arrange
        var request = new DeleteTaskCommand(Guid.NewGuid());

        var repositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.DeleteAsync(request.Id))
            .ThrowsAsync(new KeyNotFoundException("Task not found."));

        var service = new TaskService(repositoryMock.Object);

        // Act
        Func<Task> act = async () => await service.Handle(request, CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<KeyNotFoundException>()
            .WithMessage("Task not found.");

        repositoryMock.Verify(x => x.DeleteAsync(request.Id), Times.Once);
    }
}
