using ApartmentMaintenance.Domain.Common;

namespace ApartmentMaintenance.Domain.Entities;

public class Role : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<UserRoleMapping> UserRoles { get; set; } = new List<UserRoleMapping>();
}

public class UserRoleMapping : AuditableEntity<int>
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = default!;
}
