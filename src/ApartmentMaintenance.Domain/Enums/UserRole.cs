namespace ApartmentMaintenance.Domain.Enums;

public static class SystemRoles
{
    public const string SystemAdmin = "System Administrator";
    public const string President = "President";
    public const string Secretary = "Secretary";
    public const string Treasurer = "Treasurer";
    public const string MaintenanceStaff = "Maintenance Staff";
    public const string Resident = "Resident / Owner";

    public static readonly IReadOnlyList<string> All = new[]
    {
        SystemAdmin,
        President,
        Secretary,
        Treasurer,
        MaintenanceStaff,
        Resident
    };
}
