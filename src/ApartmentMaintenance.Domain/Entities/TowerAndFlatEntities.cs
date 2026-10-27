using ApartmentMaintenance.Domain.Common;
using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Domain.Entities;

public class Tower : AuditableEntity<int>
{
    public string TowerCode { get; set; } = string.Empty; // e.g., "T-A", "T-B"
    public string TowerName { get; set; } = string.Empty; // e.g., "Tower A - Maple"
    public int NumberOfFloors { get; set; }
    public int TotalFlats { get; set; }
    public string? Description { get; set; }

    // Navigation
    public ICollection<Flat> Flats { get; set; } = new List<Flat>();
    public ICollection<ParkingLot> ParkingLots { get; set; } = new List<ParkingLot>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}

public class Flat : AuditableEntity<int>
{
    public int TowerId { get; set; }
    public Tower Tower { get; set; } = default!;

    public string FlatNumber { get; set; } = string.Empty; // e.g. "101", "204"
    public int FloorNumber { get; set; }
    public decimal AreaSqFt { get; set; }
    public AreaMeasurementType AreaMeasurementType { get; set; } = AreaMeasurementType.SuperBuiltUpArea;

    public FlatOwnershipStatus OwnershipStatus { get; set; } = FlatOwnershipStatus.Unsold;
    public FlatOccupancyStatus OccupancyStatus { get; set; } = FlatOccupancyStatus.Vacant;

    // Navigation
    public ICollection<FlatOwnership> Ownerships { get; set; } = new List<FlatOwnership>();
    public ICollection<FlatOccupancy> Occupancies { get; set; } = new List<FlatOccupancy>();
    public ICollection<ParkingAllocation> ParkingAllocations { get; set; } = new List<ParkingAllocation>();
}

public class Resident : AuditableEntity<int>
{
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public ResidentType ResidentType { get; set; } = ResidentType.Owner;
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public string? CorrespondenceAddress { get; set; }
    public string Status { get; set; } = "Active";

    // Navigation
    public ICollection<FlatOwnership> FlatOwnerships { get; set; } = new List<FlatOwnership>();
    public ICollection<FlatOccupancy> FlatOccupancies { get; set; } = new List<FlatOccupancy>();
    public ICollection<ParkingAllocation> ParkingAllocations { get; set; } = new List<ParkingAllocation>();
}

public class FlatOwnership : AuditableEntity<int>
{
    public int FlatId { get; set; }
    public Flat Flat { get; set; } = default!;

    public int ResidentId { get; set; }
    public Resident Resident { get; set; } = default!;

    public decimal OwnershipPercentage { get; set; } = 100.00m;
    public bool IsPrimaryOwner { get; set; } = true;
    public DateTime StartDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndDateUtc { get; set; }
}

public class FlatOccupancy : AuditableEntity<int>
{
    public int FlatId { get; set; }
    public Flat Flat { get; set; } = default!;

    public int ResidentId { get; set; }
    public Resident Resident { get; set; } = default!;

    public ResidentType OccupancyType { get; set; } = ResidentType.Tenant;
    public DateTime MoveInDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? MoveOutDateUtc { get; set; }
}
