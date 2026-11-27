using ExpenseTracker.Infrastructure.Identity;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        // Identity creates "EmailIndex" as a normal index and checks for duplicates only in code.
        // Two registrations arriving at the same moment would both pass that check (TC-AUTH-033),
        // so the database itself must refuse a second user with the same email.
        builder.HasIndex(u => u.NormalizedEmail)
            .HasDatabaseName("EmailIndex")
            .IsUnique()
            .HasFilter("[NormalizedEmail] IS NOT NULL");
    }
}
