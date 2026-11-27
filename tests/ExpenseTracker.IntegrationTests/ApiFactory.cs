using ExpenseTracker.Infrastructure.Identity;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.IntegrationTests;

/// <summary>
/// Hosts the API in-process against the integration test database (<see cref="TestDatabase"/>).
/// The database is prepared once per run by <see cref="TestDatabase"/>, so the API does not migrate it again.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", TestDatabase.ConnectionString);
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "false");
    }

    /// <summary>
    /// Creates a user with a unique email through Identity and returns it.
    /// Every test gets its own user, so tests can run in parallel.
    /// </summary>
    public async Task<AppUser> CreateUserAsync(string password = "Lozinka123")
    {
        await using var scope = Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        var email = $"ana+{Guid.NewGuid():N}@example.com";
        var user = new AppUser { UserName = email, Email = email };

        var result = await userManager.CreateAsync(user, password);
        Assert.True(result.Succeeded, string.Join(" ", result.Errors.Select(e => e.Description)));

        return user;
    }
}
