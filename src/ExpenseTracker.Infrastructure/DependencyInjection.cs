using ExpenseTracker.Infrastructure.Identity;
using ExpenseTracker.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ExpenseTracker.Infrastructure;

public static class DependencyInjection
{
    public const string ConnectionStringName = "Default";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured. " +
                "Set it in appsettings.Development.json or via the ConnectionStrings__Default environment variable.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        // The current time for the whole app. Tests replace it with a fixed clock.
        services.TryAddSingleton(TimeProvider.System);

        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                // US-AUTH-02/AC5–AC7: locked for 5 minutes after the 5th failed attempt.
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            })
            .AddEntityFrameworkStores<AppDbContext>();

        // The password rule lives in Core (US-AUTH-01/AC5–AC7). Identity's own validator is replaced,
        // so the rule exists in one place and a short password gets one message, not two.
        services.RemoveAll<IPasswordValidator<AppUser>>();
        services.AddScoped<IPasswordValidator<AppUser>, CorePasswordValidator>();

        services.AddSingleton<DbTicketStore>();

        return services;
    }
}
