FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["TodoListApp.WebApi/TodoListApp.WebApi.csproj", "TodoListApp.WebApi/"]
COPY ["TodoListApp.Services.Database/TodoListApp.Services.Database.csproj", "TodoListApp.Services.Database/"]
COPY ["TodoListApp.Services/TodoListApp.Services.csproj", "TodoListApp.Services/"]
COPY ["TodoListApp.WebApi.Models/TodoListApp.WebApi.Models.csproj", "TodoListApp.WebApi.Models/"]
COPY ["stylecop.json", "."]

RUN dotnet restore "TodoListApp.WebApi/TodoListApp.WebApi.csproj"

COPY . .
WORKDIR "/src/TodoListApp.WebApi"
RUN dotnet publish "TodoListApp.WebApi.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TodoListApp.WebApi.dll"]
