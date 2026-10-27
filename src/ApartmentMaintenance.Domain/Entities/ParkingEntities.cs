using ApartmentMaintenance.Domain.Common;
using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Domain.Entities;

public class ParkingLot : AuditableEntity<int>
{
    public string LotNumber { get; set; } = string.Empty; // e.g. "P-B1-042"
    public int? TowerId { get; set; }
    public Tower? Tower { get; set; }

    public ParkingType ParkingType { get; set; } = ParkingType.Covered;
    public string FloorOrBasementLevel { get; set; } = "Basement 1";
    public ParkingLotStatus Status { get; set; } = ParkingLotStatus.Available;
    public string? Remarks { get; set; }

    // Navigation
    public ICollection<ParkingAllocation> Allocations { get; set; } = new List<ParkingAllocation>();
}

public class ParkingAllocation : AuditableEntity<int>
{
    public int ParkingLotId { get; set; }
    public ParkingLot ParkingLot { get; set; } = default!;

    public int FlatId { get; set; }
    public Flat Flat { get; set; } = default!;

    public int? ResidentId { get; set; }
    public Resident? Resident { get; set; }

    public string? VehicleNumber { get; set; }
    public string? VehicleType { get; set; }
    public DateTime StartDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndDateUtc { get; set; }
    public string? AllocatedBy { get; set; }
}
