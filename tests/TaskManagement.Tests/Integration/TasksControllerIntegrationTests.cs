using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TaskManagement.Application.Dtos;
using TaskManagement.Application.Interfaces;
using Xunit;

namespace TaskManagement.Tests.Integration;

public class TasksControllerIntegrationTests : IClassFixture<TasksControllerIntegrationTests.CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TasksControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllTasks_WhenCalled_ReturnsOkAndTaskCollection()
    {
        // Act
        var response = await _client.GetAsync("/api/tasks");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var tasks = await response.Content.ReadFromJsonAsync<List<TaskDto>>();
        tasks.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTaskById_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/tasks/{taskId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTask_WhenValidRequest_ReturnsCreatedAndTaskPayload()
    {
        // Arrange
        var request = new CreateTaskDto
        {
            Title = "Integration test task",
            Description = "A task created during integration testing",
            Priority = Domain.Enums.TaskPriority.Medium,
            Status = Domain.Enums.TaskStatus.Pending,
            Category = "Integration",
            DueDate = DateTime.UtcNow.AddDays(7),
            AssignedUserId = Guid.NewGuid(),
            CreatedByUserId = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdTask = await response.Content.ReadFromJsonAsync<TaskDto>();
        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be(request.Title);
        createdTask.Description.Should().Be(request.Description);
    }

    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, _ => { });

                var taskRepositoryMock = new Mock<ITaskRepository>(MockBehavior.Strict);
                taskRepositoryMock
                    .Setup(x => x.GetAllAsync())
                    .ReturnsAsync(Array.Empty<TaskDto>());
                taskRepositoryMock
                    .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync((TaskDto?)null);
                taskRepositoryMock
                    .Setup(x => x.AddAsync(It.IsAny<CreateTaskDto>()))
                    .ReturnsAsync((CreateTaskDto dto) => new TaskDto
                    {
                        Id = Guid.NewGuid(),
                        Title = dto.Title,
                        Description = dto.Description,
                        Priority = dto.Priority,
                        Status = dto.Status,
                        DueDate = dto.DueDate,
                        CreatedAt = DateTime.UtcNow
                    });
                taskRepositoryMock
                    .Setup(x => x.UpdateAsync(It.IsAny<UpdateTaskDto>()))
                    .ReturnsAsync((UpdateTaskDto dto) => new TaskDto
                    {
                        Id = dto.Id,
                        Title = dto.Title,
                        Description = dto.Description,
                        Priority = dto.Priority,
                        Status = dto.Status,
                        DueDate = dto.DueDate,
                        CreatedAt = DateTime.UtcNow
                    });
                taskRepositoryMock
                    .Setup(x => x.DeleteAsync(It.IsAny<Guid>()))
                    .Returns(Task.CompletedTask);

                services.AddSingleton(taskRepositoryMock.Object);
            });
        }
    }

    private class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string AuthenticationScheme = "Test";

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) };
            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
