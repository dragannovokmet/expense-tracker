namespace ExpenseTracker.Infrastructure.Identity;

/// <summary>
/// One sign-in. The cookie in the browser holds only <see cref="Id"/>; the session itself lives here.
/// Signing out deletes the row, so a copy of the old cookie no longer works (US-AUTH-03/AC3).
/// </summary>
public sealed class UserSession
{
    public required string Id { get; init; }

    public required string UserId { get; init; }

    /// <summary>The serialized authentication ticket (who the user is and when the session expires).</summary>
    public required byte[] Ticket { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? ExpiresAt { get; set; }
}
