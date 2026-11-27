using ExpenseTracker.Infrastructure.Persistence;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

[assembly: AssemblyFixture(typeof(ExpenseTracker.IntegrationTests.TestDatabase))]

namespace ExpenseTracker.IntegrationTests;

/// <summary>
/// The database of the integration tests. It is dropped and created again from the migrations
/// once, at the start of each test run (test plan, section 6), so every run starts clean.
/// The data is kept after the run, for debugging.
/// </summary>
/// <remarks>
/// Locally it uses SQL Server LocalDB; in CI the connection string comes from
/// the ConnectionStrings__Default environment variable (SQL Server container).
/// Tests share this database and stay independent because each test works only with
/// the data it created itself (CLAUDE.md, Test rules).
/// </remarks>
public sealed class TestDatabase : IAsyncLifetime
{
    /// <summary>The only database this class is allowed to drop.</summary>
    public const string DatabaseName = "ExpenseTracker_IntegrationTests";

    private const string LocalDbConnectionString =
        $"Server=(localdb)\\MSSQLLocalDB;Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True";

    public static string ConnectionString { get; } =
        Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? LocalDbConnectionString;

    /// <summary>A new open connection for independent checks of the database with Dapper.</summary>
    public static async Task<SqlConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection(ConnectionString);

        try
        {
            await connection.OpenAsync();
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async ValueTask InitializeAsync()
    {
        EnsureIntegrationTestDatabase();

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(ConnectionString).Options;
        await using var db = new AppDbContext(options);

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Stops the run before anything is dropped when the connection string points to another database.
    /// ConnectionStrings__Default is also the name the app itself uses, so a value set on a developer
    /// machine could otherwise make the tests drop the development database.
    /// </summary>
    private static void EnsureIntegrationTestDatabase()
    {
        var database = new SqlConnectionStringBuilder(ConnectionString).InitialCatalog;

        if (!string.Equals(database, DatabaseName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Integration tests drop and recreate their database, so they only run against '{DatabaseName}'. " +
                $"The connection string points to '{database}'. Check the ConnectionStrings__Default environment variable.");
        }
    }
}
