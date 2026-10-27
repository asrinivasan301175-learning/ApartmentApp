using ApartmentMaintenance.Domain.Entities;

namespace ApartmentMaintenance.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    RefreshToken GenerateRefreshToken(Guid userId, string? ipAddress);
    bool ValidateToken(string token);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public interface IAuditLogService
{
    Task LogAsync(string entityName, string entityId, string action, string? oldValues, string? newValues, CancellationToken cancellationToken = default);
}

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
