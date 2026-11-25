namespace ExpenseTracker.Core.Users;

/// <summary>
/// Rules for passwords (US-AUTH-01/AC5–AC7 and Notes).
/// </summary>
public static class PasswordRules
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public const string TooWeakMessage = "Password must be at least 8 characters and contain a letter and a digit.";
    public const string TooLongMessage = "Password can have at most 128 characters.";

    /// <summary>
    /// Returns an error message when the password breaks a rule, otherwise <c>null</c>.
    /// The password is never trimmed: spaces at the start or the end are part of it.
    /// </summary>
    public static string? Validate(string? password)
    {
        if (password is null || password.Length < MinLength)
        {
            return TooWeakMessage;
        }

        if (password.Length > MaxLength)
        {
            return TooLongMessage;
        }

        // Any letter counts, including local letters such as š, ć, č, đ, ž.
        // Only the digits 0–9 count as digits.
        if (!password.Any(char.IsLetter) || !password.Any(char.IsAsciiDigit))
        {
            return TooWeakMessage;
        }

        return null;
    }
}
