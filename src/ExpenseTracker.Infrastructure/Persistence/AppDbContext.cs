using ExpenseTracker.Infrastructure.Identity;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Persistence;

/// <summary>
/// EF Core database context. Entities are added feature by feature (accounts, categories, expenses...).
/// </summary>
/// <remarks>
/// <see cref="IdentityUserContext{TUser}"/> adds the Identity user tables without roles:
/// the app has no administrators or user roles.
/// </remarks>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityUserContext<AppUser>(options)
{
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity's own configuration first, so ours can change it (for example the unique email index).
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
