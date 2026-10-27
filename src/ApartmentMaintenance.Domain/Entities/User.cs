using ApartmentMaintenance.Domain.Common;

namespace ApartmentMaintenance.Domain.Entities;

public class User : AuditableEntity<Guid>
{
    public User()
    {
        Id = Guid.NewGuid();
    }

    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? LastLoginAtUtc { get; set; }

    // Navigation properties
    public ICollection<UserRoleMapping> UserRoles { get; set; } = new List<UserRoleMapping>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    
    // Optional persona links
    public Employee? EmployeeProfile { get; set; }
    public Resident? ResidentProfile { get; set; }
    public ICollection<CommitteeMember> CommitteeMemberships { get; set; } = new List<CommitteeMember>();
}
