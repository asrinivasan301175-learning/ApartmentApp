using Microsoft.Extensions.Logging;
using ApartmentMaintenance.Application.Common;
using ApartmentMaintenance.Application.Interfaces;
using ApartmentMaintenance.Domain.Entities;

namespace ApartmentMaintenance.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AuditLogService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task LogAsync(
        string entityName,
        string entityId,
        string action,
        string? oldValues,
        string? newValues,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var auditEntry = new AuditLog
            {
                EntityName = entityName,
                EntityId = entityId,
                Action = action,
                OldValues = oldValues,
                NewValues = newValues,
                UserId = _currentUserService.UserId,
                UserEmail = _currentUserService.Email,
                TimestampUtc = DateTime.UtcNow
            };

            _context.AuditLogs.Add(auditEntry);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log for entity {EntityName} with ID {EntityId}", entityName, entityId);
        }
    }
}

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        // Structured log simulation for development/production fallback
        _logger.LogInformation("DISPATCH EMAIL -> To: {ToEmail} | Subject: {Subject}", toEmail, subject);
        return Task.CompletedTask;
    }
}
