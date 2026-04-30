using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SchiperkeWebApp.Models.Database;

namespace SchiperkeWebApp.Tests.TestDoubles;

internal sealed class SqliteRepositoryTestHelper : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteRepositoryTestHelper()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public SchiperkeDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SchiperkeDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new TestSchiperkeDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private sealed class TestSchiperkeDbContext : SchiperkeDbContext
    {
        public TestSchiperkeDbContext(DbContextOptions<SchiperkeDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Keep the provider passed by the test helper and skip the scaffolded SQL Server fallback.
        }
    }
}
