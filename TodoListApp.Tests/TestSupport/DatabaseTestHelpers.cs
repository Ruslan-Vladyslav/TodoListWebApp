using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TodoListApp.Services.Database;

namespace TodoListApp.Tests.TestSupport;

internal static class DatabaseTestHelpers
{
    public static TodoListDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TodoListDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new TodoListDbContext(options);
    }
}
