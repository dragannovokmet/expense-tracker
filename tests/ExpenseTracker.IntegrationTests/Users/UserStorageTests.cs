using Dapper;

using ExpenseTracker.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.IntegrationTests.Users;

public sealed class UserStorageTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // Copied from the acceptance criterion (US-AUTH-01/AC5), not taken from the production code.
    private const string TooWeakMessage = "Password must be at least 8 characters and contain a letter and a digit.";

    // TC-AUTH-012 (database part): the password is stored only as a hash.
    [Fact]
    public async Task Password_is_stored_only_as_a_hash()
    {
        var user = await factory.CreateUserAsync(password: "Lozinka123");

        await using var db = await TestDatabase.OpenConnectionAsync();
        var storedHash = await db.QuerySingleAsync<string>(
            "SELECT PasswordHash FROM AspNetUsers WHERE Id = @Id", new { user.Id });

        Assert.DoesNotContain("Lozinka123", storedHash, StringComparison.OrdinalIgnoreCase);

        // Positive control: the hash still belongs to that password.
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var storedUser = await userManager.FindByIdAsync(user.Id);
        Assert.True(await userManager.CheckPasswordAsync(storedUser!, "Lozinka123"));
    }

    // TC-AUTH-033 (database part): the database itself refuses a second user with the same email.
    // The row is written with plain SQL, past all checks in the code, as a second request would be
    // when it arrives at the same moment as the first one.
    [Fact]
    public async Task Database_rejects_a_second_user_with_the_same_email()
    {
        var user = await factory.CreateUserAsync();

        await using var db = await TestDatabase.OpenConnectionAsync();
        var insertDuplicate = () => db.ExecuteAsync(
            """
            INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail,
                EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
            VALUES (@Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, 0, 0, 0, 1, 0)
            """,
            new
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "other-" + user.UserName,
                NormalizedUserName = "OTHER-" + user.NormalizedUserName,
                user.Email,
                user.NormalizedEmail,
            });

        var exception = await Assert.ThrowsAsync<SqlException>(insertDuplicate);

        Assert.Equal(2601, exception.Number);              // duplicate key in a unique index
        Assert.Contains("EmailIndex", exception.Message);
    }

    // The password rule from Core is the only rule Identity uses (one message, not two).
    [Fact]
    public async Task Identity_uses_the_password_rule_from_core()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var email = $"ana+{Guid.NewGuid():N}@example.com";

        var result = await userManager.CreateAsync(new AppUser { UserName = email, Email = email }, "abcdef1");

        Assert.False(result.Succeeded);
        var error = Assert.Single(result.Errors);
        Assert.Equal(TooWeakMessage, error.Description);
    }
}
