# Todo List Application (.NET 9, ASP.NET Core MVC, Web API)

[![.NET 9](https://img.shields.io/badge/.NET-9.0-blueviolet?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-MVC%20%2B%20Web%20API-blue?logo=dotnet&logoColor=white)](https://learn.microsoft.com/aspnet/core)
[![SQL Server](https://img.shields.io/badge/Database-SQL_Server-red?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)

## Overview

This project is a full-stack **Todo List** web application built with **ASP.NET Core MVC**, **ASP.NET Core Web API**, **Entity Framework Core**, **ASP.NET Core Identity**, and **SQL Server**.

The application lets users create todo lists and tasks, share lists with other users, assign roles, send invitations, add tags and comments, and receive notifications for collaboration events.

## Features

- User registration, login, password reset, and profile management
- Todo list CRUD operations
- Todo task CRUD operations
- Shared todo lists with roles: owner, editor, viewer
- Invitations for list sharing
- Task assignment and completion notifications
- Tags and comments for tasks
- MVC web frontend backed by a separate Web API
- Unit tests for WebApp controllers, WebApi controllers, and database services

## Technologies

- **.NET 9**
- **ASP.NET Core MVC**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **ASP.NET Core Identity**
- **SQL Server**
- **xUnit**
- **Moq**
- **FluentAssertions**

## Solution Structure

```text
TodoListApp.WebApp              MVC frontend
TodoListApp.WebApi              REST API
TodoListApp.Services            Shared service interfaces and auth service
TodoListApp.Services.Database   EF Core database services, entities, migrations
TodoListApp.Services.WebApi     WebApp HTTP client service implementations
TodoListApp.WebApi.Models       Shared DTOs, enums, request/response models
TodoListApp.Tests               Unit and database-service tests
```

## Run Locally Without Docker

Prerequisites:

- .NET 9 SDK
- SQL Server or SQL Server Express

1. Clone the repository.

2. Configure local settings:

- `TodoListApp.WebApi` -> `ConnectionStrings:TodoListDb`
- `TodoListApp.WebApp` -> `WebApi:ApiUri`

Example API connection string:

```json
"TodoListDb": "Server=localhost\\SQLEXPRESS;Database=TodoListDb;Trusted_Connection=True;TrustServerCertificate=True;"
```

Example WebApp API URL:

```json
"ApiUri": "https://localhost:7292/"
```

3. Apply EF Core migrations:

```bash
dotnet ef database update --project TodoListApp.Services.Database --startup-project TodoListApp.WebApi
```

4. Start the Web API:

```bash
dotnet run --project TodoListApp.WebApi
```

5. Start the MVC WebApp in another terminal:

```bash
dotnet run --project TodoListApp.WebApp
```

6. Open the WebApp URL shown in the terminal.

## Run With Docker

See the full Docker guide here:

[DOCKER.md](DOCKER.md)

## Run Tests

```bash
dotnet test TodoListApp.Tests/TodoListApp.Tests.csproj
```
