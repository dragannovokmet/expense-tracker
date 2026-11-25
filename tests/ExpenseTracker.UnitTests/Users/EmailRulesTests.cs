using ExpenseTracker.Core.Users;

namespace ExpenseTracker.UnitTests.Users;

public sealed class EmailRulesTests
{
    // Copied from the acceptance criterion (US-AUTH-01/AC4), not taken from the production code.
    private const string InvalidEmailMessage = "Enter a valid email address.";

    // TC-AUTH-005, equivalence partitioning: valid partition.
    [Theory]
    [InlineData("ana@example.com")]
    [InlineData("ana.maric+test@mail.example.com")]
    [InlineData("  ana@example.com  ")]
    public void Valid_email_is_accepted(string email)
    {
        Assert.Null(EmailRules.Validate(email));
    }

    // TC-AUTH-005, equivalence partitioning and error guessing: invalid partitions.
    [Theory]
    [InlineData("ana@")]                 // no domain
    [InlineData("ana.example.com")]      // no @
    [InlineData("@example.com")]         // nothing before @
    [InlineData("ana@@example.com")]     // two @
    [InlineData("ana@example")]          // domain without a dot
    [InlineData("ana maric@example.com")] // space inside
    [InlineData("")]                     // empty
    [InlineData("   ")]                  // spaces only
    [InlineData(null)]
    public void Invalid_email_is_rejected(string? email)
    {
        Assert.Equal(InvalidEmailMessage, EmailRules.Validate(email));
    }

    // Boundary value analysis on the maximum length (254 characters).
    [Theory]
    [InlineData(254, true)]
    [InlineData(255, false)]
    public void Email_length_is_limited_to_254_characters(int length, bool isValid)
    {
        const string domain = "@example.com";
        var email = new string('a', length - domain.Length) + domain;

        Assert.Equal(isValid, EmailRules.Validate(email) is null);
    }

    // TC-AUTH-004 (unit part): surrounding spaces and letter case are removed.
    [Theory]
    [InlineData(" Ana@Example.COM ", "ana@example.com")]
    [InlineData("ANA@EXAMPLE.COM", "ana@example.com")]
    [InlineData("ana@example.com", "ana@example.com")]
    public void Email_is_normalized_to_trimmed_lower_case(string email, string expected)
    {
        Assert.Equal(expected, EmailRules.Normalize(email));
    }
}
