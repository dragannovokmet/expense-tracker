using System.Security.Claims;

using Dapper;

using ExpenseTracker.Infrastructure.Identity;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace ExpenseTracker.IntegrationTests.Users;

/// <summary>
/// The server-side session store. It is the basis for TC-AUTH-026:
/// after sign-out, a saved copy of the old cookie must not work.
/// </summary>
public sealed class SessionStoreTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static readonly DateTimeOffset Now = new(2026, 11, 25, 10, 0, 0, TimeSpan.Zero);

    private readonly FakeTimeProvider clock = new(Now);

    [Fact]
    public async Task Stored_session_can_be_read_back()
    {
        var user = await factory.CreateUserAsync();
        var store = CreateStore();

        var key = await store.StoreAsync(CreateTicket(user, expiresAt: Now.AddHours(1)));
        var ticket = await store.RetrieveAsync(key);

        Assert.NotNull(ticket);
        Assert.Equal(user.Id, ticket.Principal.FindFirstValue(ClaimTypes.NameIdentifier));
    }

    // Basis for TC-AUTH-026: a removed session cannot be used again.
    [Fact]
    public async Task Removed_session_cannot_be_read()
    {
        var user = await factory.CreateUserAsync();
        var store = CreateStore();
        var key = await store.StoreAsync(CreateTicket(user, expiresAt: Now.AddHours(1)));

        await store.RemoveAsync(key);

        Assert.Null(await store.RetrieveAsync(key));
        await using var db = await TestDatabase.OpenConnectionAsync();
        Assert.Equal(0, await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM UserSessions WHERE Id = @key", new { key }));
    }

    // Decision B: signing out ends only the current session, not the user's other sessions.
    [Fact]
    public async Task Removing_one_session_keeps_the_other_sessions_of_the_user()
    {
        var user = await factory.CreateUserAsync();
        var store = CreateStore();
        var laptop = await store.StoreAsync(CreateTicket(user, expiresAt: Now.AddHours(1)));
        var phone = await store.StoreAsync(CreateTicket(user, expiresAt: Now.AddHours(1)));

        await store.RemoveAsync(laptop);

        Assert.Null(await store.RetrieveAsync(laptop));
        Assert.NotNull(await store.RetrieveAsync(phone));
    }

    // Boundary value analysis on the expiry time: one second before it, and exactly at it.
    [Theory]
    [InlineData(-1, true)]
    [InlineData(0, false)]
    public async Task Session_is_valid_only_until_it_expires(int secondsFromExpiry, bool isValid)
    {
        var user = await factory.CreateUserAsync();
        var store = CreateStore();
        var expiresAt = Now.AddHours(1);
        var key = await store.StoreAsync(CreateTicket(user, expiresAt));

        clock.SetUtcNow(expiresAt.AddSeconds(secondsFromExpiry));

        Assert.Equal(isValid, await store.RetrieveAsync(key) is not null);
    }

    private DbTicketStore CreateStore() =>
        new(factory.Services.GetRequiredService<IServiceScopeFactory>(), clock);

    private static AuthenticationTicket CreateTicket(AppUser user, DateTimeOffset expiresAt)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id)], "Test");
        var properties = new AuthenticationProperties { ExpiresUtc = expiresAt };
        return new AuthenticationTicket(new ClaimsPrincipal(identity), properties, "Test");
    }
}
