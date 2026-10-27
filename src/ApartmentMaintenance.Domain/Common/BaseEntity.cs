namespace ApartmentMaintenance.Domain.Common;

public abstract class BaseEntity<TId>
{
    public TId Id { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}

public abstract class BaseEntity : BaseEntity<int>
{
}
