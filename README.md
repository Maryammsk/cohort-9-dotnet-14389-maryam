# TaskManagementTool

TaskManagementTool is a clean architecture ASP.NET Core application for task management, organized with a separation of concerns between API, application, domain, and infrastructure layers.

## Architecture

- `src/Presentation/TaskManagement.API` - ASP.NET Core Web API entry point
- `src/Core/TaskManagement.Application` - application services, commands, and business logic
- `src/Core/TaskManagement.Domain` - domain entities, value objects, and domain rules
- `src/Infrastructure/TaskManagement.Infrastructure` - database access, EF Core persistence, and infrastructure services
- `src/Infrastructure/TaskManagement.Identity` - identity and authentication-related implementation
- `tests/TaskManagement.Tests` - unit tests for application and domain behavior

## Tech Stack

- ASP.NET Core 10
- Entity Framework Core SQL Server
- Serilog logging
- Swashbuckle / Swagger for API documentation
- C# 12 with nullable reference types enabled

## Setup

1. Restore NuGet packages:

```bash
dotnet restore TaskManagementTool.slnx
```

2. Build the solution:

```bash
dotnet build TaskManagementTool.slnx
```

3. Run the API project:

```bash
cd src/Presentation/TaskManagement.API
dotnet run
```

4. Open Swagger UI:

- Navigate to `https://localhost:5001/swagger` or the configured host URL.

## Notes

- Use the root `.editorconfig` for consistent C# and JavaScript formatting.
- Add database connection settings in `appsettings.json` before running EF Core migrations.
