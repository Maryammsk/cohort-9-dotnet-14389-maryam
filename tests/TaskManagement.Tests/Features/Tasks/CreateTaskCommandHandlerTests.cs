using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation;
using Moq;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Features.Tasks.Commands;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Application.Validators;
using Xunit;

namespace TaskManagement.Tests.Features.Tasks;

public class CreateTaskCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTaskIsCreated_ReturnsCreatedTask()
    {
        // Arrange
        var request = new CreateTaskCommand(new CreateTaskDto
        {
            Title = "New task",
            Description = "A sample task",
            Priority = Domain.Enums.TaskPriority.High,
            Status = Domain.Enums.TaskStatus.Pending,
            Category = "Work",
            DueDate = DateTime.UtcNow.AddDays(3),
            AssignedUserId = Guid.NewGuid(),
            CreatedByUserId = Guid.NewGuid()
        });

        var expectedTask = new TaskDto
        {
            Id = Guid.NewGuid(),
            Title = request.Task.Title,
            Description = request.Task.Description,
            Priority = request.Task.Priority,
            Status = request.Task.Status,
            DueDate = request.Task.DueDate,
            CreatedAt = DateTime.UtcNow
        };

        var repositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.AddAsync(request.Task))
            .ReturnsAsync(expectedTask);

        var service = new TaskService(repositoryMock.Object);

        // Act
        var actual = await service.Handle(request, CancellationToken.None);

        // Assert
        actual.Should().BeEquivalentTo(expectedTask);
        repositoryMock.Verify(x => x.AddAsync(request.Task), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        var request = new CreateTaskCommand(new CreateTaskDto
        {
            Title = "New task",
            DueDate = DateTime.UtcNow.AddDays(1),
            AssignedUserId = Guid.NewGuid(),
            CreatedByUserId = Guid.NewGuid()
        });

        var repositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
        repositoryMock
            .Setup(x => x.AddAsync(request.Task))
            .ThrowsAsync(new InvalidOperationException("Repository failure"));

        var service = new TaskService(repositoryMock.Object);

        // Act
        Func<Task> act = async () => await service.Handle(request, CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Repository failure");

        repositoryMock.Verify(x => x.AddAsync(request.Task), Times.Once);
    }

    [Fact]
    public void Validator_WhenTitleIsEmpty_IsInvalid()
    {
        // Arrange
        var command = new CreateTaskCommand(new CreateTaskDto
        {
            Title = string.Empty,
            DueDate = DateTime.UtcNow.AddDays(1)
        });

        var validator = new CreateTaskCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Task.Title" && x.ErrorMessage == "Title is required.");
    }

    [Fact]
    public void Validator_WhenDueDateIsInPast_IsInvalid()
    {
        // Arrange
        var command = new CreateTaskCommand(new CreateTaskDto
        {
            Title = "Task with past due date",
            DueDate = DateTime.UtcNow.AddDays(-1)
        });

        var validator = new CreateTaskCommandValidator();

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.PropertyName == "Task.DueDate" && x.ErrorMessage == "DueDate must be a future date.");
    }
}
