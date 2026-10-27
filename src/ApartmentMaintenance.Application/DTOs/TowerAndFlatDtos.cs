using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Application.DTOs;

public class TowerDto
{
    public int Id { get; set; }
    public string TowerCode { get; set; } = string.Empty;
    public string TowerName { get; set; } = string.Empty;
    public int NumberOfFloors { get; set; }
    public int TotalFlats { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int OccupiedFlatsCount { get; set; }
    public int VacantFlatsCount { get; set; }
}

public class CreateTowerDto
{
    public string TowerCode { get; set; } = string.Empty;
    public string TowerName { get; set; } = string.Empty;
    public int NumberOfFloors { get; set; }
    public int TotalFlats { get; set; }
    public string? Description { get; set; }
}

public class FlatDto
{
    public int Id { get; set; }
    public int TowerId { get; set; }
    public string TowerName { get; set; } = string.Empty;
    public string TowerCode { get; set; } = string.Empty;
    public string FlatNumber { get; set; } = string.Empty;
    public int FloorNumber { get; set; }
    public decimal AreaSqFt { get; set; }
    public string AreaMeasurementType { get; set; } = string.Empty;
    public string OwnershipStatus { get; set; } = string.Empty;
    public string OccupancyStatus { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public List<FlatOwnerSummaryDto> Owners { get; set; } = new();
    public List<FlatOccupantSummaryDto> Occupants { get; set; } = new();
    public List<string> AllocatedParkingLots { get; set; } = new();
}

public class CreateFlatDto
{
    public int TowerId { get; set; }
    public string FlatNumber { get; set; } = string.Empty;
    public int FloorNumber { get; set; }
    public decimal AreaSqFt { get; set; }
    public AreaMeasurementType AreaMeasurementType { get; set; } = AreaMeasurementType.SuperBuiltUpArea;
    public FlatOwnershipStatus OwnershipStatus { get; set; } = FlatOwnershipStatus.Unsold;
    public FlatOccupancyStatus OccupancyStatus { get; set; } = FlatOccupancyStatus.Vacant;
}

public class FlatOwnerSummaryDto
{
    public int ResidentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public decimal OwnershipPercentage { get; set; }
    public bool IsPrimaryOwner { get; set; }
    public DateTime StartDateUtc { get; set; }
}

public class FlatOccupantSummaryDto
{
    public int ResidentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string OccupancyType { get; set; } = string.Empty;
    public DateTime MoveInDateUtc { get; set; }
}

public class ResidentDto
{
    public int Id { get; set; }
    public Guid? UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string ResidentType { get; set; } = string.Empty;
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public string? CorrespondenceAddress { get; set; }
    public string Status { get; set; } = "Active";
    public List<string> AssociatedFlats { get; set; } = new();
}

public class CreateResidentDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public ResidentType ResidentType { get; set; } = ResidentType.Owner;
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public string? CorrespondenceAddress { get; set; }
    public int? InitialFlatId { get; set; }
    public bool CreateLoginAccount { get; set; }
}

public class AssignOwnershipDto
{
    public int FlatId { get; set; }
    public int ResidentId { get; set; }
    public decimal OwnershipPercentage { get; set; } = 100m;
    public bool IsPrimaryOwner { get; set; } = true;
}

public class AssignOccupancyDto
{
    public int FlatId { get; set; }
    public int ResidentId { get; set; }
    public ResidentType OccupancyType { get; set; } = ResidentType.Tenant;
}
