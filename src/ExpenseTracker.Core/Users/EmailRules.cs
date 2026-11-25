using System.Text.RegularExpressions;

namespace ExpenseTracker.Core.Users;

/// <summary>
/// Rules for the email address used as the user name (US-AUTH-01/AC3, AC4).
/// </summary>
public static partial class EmailRules
{
    /// <summary>Maximum length of an email address (RFC 5321).</summary>
    public const int MaxLength = 254;

    public const string InvalidMessage = "Enter a valid email address.";

    /// <summary>
    /// Returns the form in which an email is stored and compared:
    /// without surrounding spaces and in lower case. "  Ana@Example.COM " becomes "ana@example.com".
    /// </summary>
    public static string Normalize(string email) => email.Trim().ToLowerInvariant();

    /// <summary>
    /// Returns <see cref="InvalidMessage"/> when the email is not valid, otherwise <c>null</c>.
    /// Surrounding spaces are ignored, because the email is normalized before it is stored.
    /// </summary>
    public static string? Validate(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return InvalidMessage;
        }

        var trimmed = email.Trim();

        if (trimmed.Length > MaxLength || !EmailPattern().IsMatch(trimmed))
        {
            return InvalidMessage;
        }

        return null;
    }

    // One "@", a non-empty part before it, and a domain with at least one dot.
    // No spaces and no second "@" anywhere.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
