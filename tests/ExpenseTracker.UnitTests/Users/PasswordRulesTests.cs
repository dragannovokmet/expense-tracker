using ExpenseTracker.Core.Users;

namespace ExpenseTracker.UnitTests.Users;

public sealed class PasswordRulesTests
{
    // Expected messages are copied from the acceptance criteria (US-AUTH-01/AC5 and Notes),
    // not taken from the production code, so a changed message makes the tests fail.
    private const string TooWeakMessage = "Password must be at least 8 characters and contain a letter and a digit.";
    private const string TooLongMessage = "Password can have at most 128 characters.";

    // TC-AUTH-007, boundary value analysis on the minimum length (8).
    [Theory]
    [InlineData("abcdef1")]  // 7 characters: just below the boundary
    [InlineData("")]
    [InlineData(null)]
    public void Password_shorter_than_8_characters_is_rejected(string? password)
    {
        Assert.Equal(TooWeakMessage, PasswordRules.Validate(password));
    }

    // TC-AUTH-007, boundary value analysis: both boundaries of the allowed range (8 and 128).
    [Theory]
    [InlineData(8)]
    [InlineData(128)]
    public void Password_at_a_length_boundary_is_accepted(int length)
    {
        var password = new string('a', length - 1) + "1";

        Assert.Null(PasswordRules.Validate(password));
    }

    // TC-AUTH-007, boundary value analysis: just above the maximum length (129).
    [Fact]
    public void Password_longer_than_128_characters_is_rejected()
    {
        var password = new string('a', 128) + "1";

        Assert.Equal(TooLongMessage, PasswordRules.Validate(password));
    }

    // TC-AUTH-008, equivalence partitioning: missing letter or missing digit.
    [Theory]
    [InlineData("abcdefgh")]  // no digit
    [InlineData("12345678")]  // no letter
    [InlineData("!@#$%^&*")]  // neither
    public void Password_without_a_letter_or_a_digit_is_rejected(string password)
    {
        Assert.Equal(TooWeakMessage, PasswordRules.Validate(password));
    }

    // TC-AUTH-008, equivalence partitioning: valid partitions, including local letters.
    [Theory]
    [InlineData("abcdefg1")]
    [InlineData("šćčđžab1")]
    [InlineData("Lozinka123")]
    public void Password_with_a_letter_and_a_digit_is_accepted(string password)
    {
        Assert.Null(PasswordRules.Validate(password));
    }

    // TC-AUTH-034 (unit part): spaces are part of the password and are counted.
    [Fact]
    public void Password_is_not_trimmed()
    {
        // 7 visible characters plus a space at each end: 9 characters, so it is accepted.
        Assert.Null(PasswordRules.Validate(" abcdef1 "));
    }
}
