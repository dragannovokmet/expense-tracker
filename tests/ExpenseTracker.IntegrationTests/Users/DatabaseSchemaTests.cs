using Dapper;

namespace ExpenseTracker.IntegrationTests.Users;

/// <summary>
/// Checks what the migrations created, with plain SQL against the database
/// (not through EF Core, which would only repeat the model of the app).
/// </summary>
public sealed class DatabaseSchemaTests
{
    [Fact]
    public async Task Migrations_create_the_user_and_session_tables()
    {
        await using var db = await TestDatabase.OpenConnectionAsync();

        var tables = await db.QueryAsync<string>("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES");

        Assert.Subset(
            tables.ToHashSet(),
            new HashSet<string> { "AspNetUsers", "AspNetUserClaims", "AspNetUserLogins", "AspNetUserTokens", "UserSessions" });
    }

    [Fact]
    public async Task Role_tables_are_not_created()
    {
        await using var db = await TestDatabase.OpenConnectionAsync();

        var roleTables = await db.QueryAsync<string>(
            "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'AspNet%Role%'");

        Assert.Empty(roleTables);
    }

    // TC-AUTH-033 (database part): only a unique index stops two registrations that arrive at the same moment.
    [Fact]
    public async Task Email_index_is_unique()
    {
        await using var db = await TestDatabase.OpenConnectionAsync();

        var isUnique = await db.QuerySingleAsync<bool>(
            "SELECT is_unique FROM sys.indexes WHERE name = 'EmailIndex' AND object_id = OBJECT_ID('AspNetUsers')");

        Assert.True(isUnique);
    }
}
