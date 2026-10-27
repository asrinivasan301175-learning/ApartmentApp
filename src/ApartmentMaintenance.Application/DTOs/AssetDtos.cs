using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Application.DTOs;

public class AssetDto
{
    public int Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public int? TowerId { get; set; }
    public string? TowerName { get; set; }
    public string? SpecificLocation { get; set; }
    public int? AssignedVendorId { get; set; }
    public string? AssignedVendorName { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextScheduledMaintenanceDate { get; set; }
    public bool IsActive { get; set; }
}

public class CreateAssetDto
{
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }
    public int? TowerId { get; set; }
    public string? SpecificLocation { get; set; }
    public int? AssignedVendorId { get; set; }
    public AssetCondition Condition { get; set; } = AssetCondition.Good;
    public DateTime? NextScheduledMaintenanceDate { get; set; }
}

public class AssetMaintenanceRecordDto
{
    public int Id { get; set; }
    public int AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public DateTime MaintenanceDate { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public string PerformedBy { get; set; } = string.Empty;
    public int? VendorId { get; set; }
    public string? VendorName { get; set; }
    public decimal CostIncurred { get; set; }
    public string FindingsAndWorkDone { get; set; } = string.Empty;
    public string? PartsReplaced { get; set; }
    public DateTime? NextRecommendedServiceDate { get; set; }
}

public class CreateAssetMaintenanceRecordDto
{
    public int AssetId { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public MaintenanceType MaintenanceType { get; set; } = MaintenanceType.RoutineInspection;
    public string PerformedBy { get; set; } = string.Empty;
    public int? VendorId { get; set; }
    public decimal CostIncurred { get; set; }
    public string FindingsAndWorkDone { get; set; } = string.Empty;
    public string? PartsReplaced { get; set; }
    public DateTime? NextRecommendedServiceDate { get; set; }
}
