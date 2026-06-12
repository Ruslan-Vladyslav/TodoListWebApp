# Running With Docker

This project runs as three containers:

- `webapp`: the MVC web application
- `webapi`: the API used by the web app
- `sqlserver`: SQL Server 2022 for application data

## Prerequisites

- Docker Desktop or Docker Engine with Docker Compose

## Start The App

From the repository root:

```bash
docker compose up --build
```

Open the web app:

```text
http://localhost:8081
```

Open the API/Swagger UI:

```text
http://localhost:5080
```

The API applies EF Core migrations automatically when `Database__AutoMigrate=true`, which is already set in `docker-compose.yml`.

## Stop The App

```bash
docker compose down
```

To remove the SQL Server data volume too:

```bash
docker compose down -v
```

## Useful Commands

Rebuild after code changes:

```bash
docker compose up --build
```

View logs:

```bash
docker compose logs -f
```

Run tests locally:

```bash
dotnet test TodoListApp.Tests/TodoListApp.Tests.csproj
```

## Notes

The SQL Server password in `docker-compose.yml` is for local development only. Change it before using this setup anywhere outside a local machine.
