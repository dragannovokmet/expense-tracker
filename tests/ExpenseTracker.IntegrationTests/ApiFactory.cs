using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ExpenseTracker.IntegrationTests;

/// <summary>
/// Hosts the API in-process against a dedicated test database.
/// Locally it uses SQL Server LocalDB; in CI the connection string comes from
/// the ConnectionStrings__Default environment variable (SQL Server container).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private const string LocalDbConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=ExpenseTracker_IntegrationTests;Trusted_Connection=True;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? LocalDbConnectionString;

        builder.UseSetting("ConnectionStrings:Default", connectionString);
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
    }
}
