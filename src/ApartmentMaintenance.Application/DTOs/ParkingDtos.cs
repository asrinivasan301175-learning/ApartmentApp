using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Application.DTOs;

public class ParkingLotDto
{
    public int Id { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public int? TowerId { get; set; }
    public string? TowerName { get; set; }
    public string ParkingType { get; set; } = string.Empty;
    public string FloorOrBasementLevel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public bool IsActive { get; set; }

    // Current active allocation
    public int? AllocatedFlatId { get; set; }
    public string? AllocatedFlatNumber { get; set; }
    public string? AllocatedResidentName { get; set; }
    public string? VehicleNumber { get; set; }
    public DateTime? AllocationStartDateUtc { get; set; }
}

public class CreateParkingLotDto
{
    public string LotNumber { get; set; } = string.Empty;
    public int? TowerId { get; set; }
    public ParkingType ParkingType { get; set; } = ParkingType.Covered;
    public string FloorOrBasementLevel { get; set; } = "Basement 1";
    public string? Remarks { get; set; }
}

public class AllocateParkingDto
{
    public int ParkingLotId { get; set; }
    public int FlatId { get; set; }
    public int? ResidentId { get; set; }
    public string? VehicleNumber { get; set; }
    public string? VehicleType { get; set; }
}

public class ParkingAllocationHistoryDto
{
    public int Id { get; set; }
    public string LotNumber { get; set; } = string.Empty;
    public string FlatNumber { get; set; } = string.Empty;
    public string ResidentName { get; set; } = string.Empty;
    public string? VehicleNumber { get; set; }
    public DateTime StartDateUtc { get; set; }
    public DateTime? EndDateUtc { get; set; }
    public bool IsActive { get; set; }
}
