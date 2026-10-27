using ApartmentMaintenance.Domain.Common;
using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Domain.Entities;

public class AssetCategory : AuditableEntity<int>
{
    public string Name { get; set; } = string.Empty; // e.g. Lifts, Generators, Pumps, Fire Safety, CCTV, Gym
    public string? Description { get; set; }

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}

public class Asset : AuditableEntity<int>
{
    public string AssetCode { get; set; } = string.Empty; // e.g. "AST-GEN-01"
    public string AssetName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public AssetCategory Category { get; set; } = default!;

    public string? Description { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal PurchaseCost { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }

    public int? TowerId { get; set; }
    public Tower? Tower { get; set; }
    public string? SpecificLocation { get; set; } // e.g. "Roof Top", "Basement Pump Room"

    public int? AssignedVendorId { get; set; }
    public Vendor? AssignedVendor { get; set; }

    public AssetCondition Condition { get; set; } = AssetCondition.Good;
    public string Status { get; set; } = "Operational";
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextScheduledMaintenanceDate { get; set; }

    // Navigation
    public ICollection<AssetMaintenanceRecord> MaintenanceRecords { get; set; } = new List<AssetMaintenanceRecord>();
}

public class AssetMaintenanceRecord : AuditableEntity<int>
{
    public int AssetId { get; set; }
    public Asset Asset { get; set; } = default!;

    public DateTime MaintenanceDate { get; set; }
    public MaintenanceType MaintenanceType { get; set; } = MaintenanceType.RoutineInspection;
    public string PerformedBy { get; set; } = string.Empty; // Staff or Vendor Name
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public decimal CostIncurred { get; set; }
    public string FindingsAndWorkDone { get; set; } = string.Empty;
    public string? PartsReplaced { get; set; }
    public DateTime? NextRecommendedServiceDate { get; set; }
    public string? AttachmentUrl { get; set; }
}
