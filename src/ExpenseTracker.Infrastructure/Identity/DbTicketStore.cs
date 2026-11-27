using System.Security.Claims;

using ExpenseTracker.Infrastructure.Persistence;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.Infrastructure.Identity;

/// <summary>
/// Server-side session store for the authentication cookie.
/// Without it the cookie carries the whole ticket, and a copy saved before sign-out
/// stays valid until it expires. With it the cookie carries only a session ID,
/// and signing out removes the session from the database.
/// </summary>
/// <remarks>
/// The cookie handler keeps one instance for the whole app, while <see cref="AppDbContext"/>
/// is created per request. That is why each call creates its own scope.
/// </remarks>
public sealed class DbTicketStore(IServiceScopeFactory scopeFactory, TimeProvider timeProvider) : ITicketStore
{
    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var session = new UserSession
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = GetUserId(ticket),
            Ticket = TicketSerializer.Default.Serialize(ticket),
            CreatedAt = timeProvider.GetUtcNow(),
            ExpiresAt = ticket.Properties.ExpiresUtc,
        };

        db.UserSessions.Add(session);
        await db.SaveChangesAsync();

        return session.Id;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var session = await db.UserSessions.FindAsync(key);
        if (session is null)
        {
            return;
        }

        session.Ticket = TicketSerializer.Default.Serialize(ticket);
        session.ExpiresAt = ticket.Properties.ExpiresUtc;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Returns the ticket for a session ID, or <c>null</c> when the session does not exist
    /// (signed out) or has expired.
    /// </summary>
    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var session = await db.UserSessions.AsNoTracking().SingleOrDefaultAsync(s => s.Id == key);
        if (session is null)
        {
            return null;
        }

        if (session.ExpiresAt is { } expiresAt && expiresAt <= timeProvider.GetUtcNow())
        {
            return null;
        }

        return TicketSerializer.Default.Deserialize(session.Ticket);
    }

    public async Task RemoveAsync(string key)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.UserSessions.Where(s => s.Id == key).ExecuteDeleteAsync();
    }

    private static string GetUserId(AuthenticationTicket ticket) =>
        ticket.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("The authentication ticket has no user ID.");
}
