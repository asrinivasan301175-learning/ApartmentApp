using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApartmentMaintenance.Application.Interfaces;
using ApartmentMaintenance.Infrastructure.Persistence;
using ApartmentMaintenance.Infrastructure.Services;

namespace ApartmentMaintenance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbProvider = configuration["DatabaseProvider"] ?? "MySQL";
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? "Server=localhost;Port=3306;Database=ApartmentMaintenanceDb;Uid=root;Pwd=root;";

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase) ||
                connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseMySQL(connectionString);
            }
        });

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
