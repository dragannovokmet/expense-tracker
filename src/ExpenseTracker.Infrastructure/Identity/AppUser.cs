using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Identity;

/// <summary>
/// A registered user. Identity provides the email, the password hash and the lockout fields.
/// Own fields (for example the time zone) are added here when a story needs them.
/// </summary>
public sealed class AppUser : IdentityUser
{
}
