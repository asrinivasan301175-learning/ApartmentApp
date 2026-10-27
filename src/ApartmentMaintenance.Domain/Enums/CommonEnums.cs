namespace ApartmentMaintenance.Domain.Enums;

public enum ResidentType
{
    Owner = 1,
    Tenant = 2,
    FamilyMember = 3,
    Other = 4
}

public enum AreaMeasurementType
{
    CarpetArea = 1,
    SuperBuiltUpArea = 2
}

public enum FlatOwnershipStatus
{
    Sold = 1,
    Unsold = 2,
    UnderMortgage = 3,
    SocietyOwned = 4
}

public enum FlatOccupancyStatus
{
    SelfOccupied = 1,
    TenantOccupied = 2,
    Vacant = 3,
    UnderRenovation = 4
}

public enum ParkingType
{
    Open = 1,
    Covered = 2,
    Basement1 = 3,
    Basement2 = 4,
    Visitor = 5,
    TwoWheeler = 6
}

public enum ParkingLotStatus
{
    Available = 1,
    Allocated = 2,
    Reserved = 3,
    UnderMaintenance = 4
}

public enum EmploymentType
{
    FullTime = 1,
    PartTime = 2,
    Contract = 3,
    Temporary = 4
}

public enum EmploymentStatus
{
    Active = 1,
    Probation = 2,
    OnLeave = 3,
    Resigned = 4,
    Terminated = 5
}

public enum VendorCategoryType
{
    SecurityServices = 1,
    LiftMaintenance = 2,
    CleaningServices = 3,
    Plumbing = 4,
    ElectricalMaintenance = 5,
    GardeningAndLandscaping = 6,
    PestControl = 7,
    FireSafety = 8,
    GeneralContractors = 9,
    WaterTreatment = 10,
    WasteManagement = 11,
    Other = 99
}

public enum ContractStatus
{
    Draft = 1,
    Active = 2,
    PendingRenewal = 3,
    Expired = 4,
    Terminated = 5
}

public enum AssetCondition
{
    Excellent = 1,
    Good = 2,
    Fair = 3,
    NeedsMaintenance = 4,
    UnderRepair = 5,
    Decommissioned = 6
}

public enum MaintenanceType
{
    RoutineInspection = 1,
    PreventativeMaintenance = 2,
    EmergencyRepair = 3,
    StatutoryComplianceCheck = 4,
    WarrantyService = 5
}

public enum PurchaseRequestStatus
{
    Draft = 1,
    PendingApproval = 2,
    ReturnedForChanges = 3,
    Approved = 4,
    Rejected = 5,
    Cancelled = 6
}

public enum ApprovalDecision
{
    Approved = 1,
    Rejected = 2,
    ReturnedForChanges = 3
}

public enum PurchaseOrderStatus
{
    Issued = 1,
    Acknowledged = 2,
    PartiallyDelivered = 3,
    Delivered = 4,
    Closed = 5,
    Cancelled = 6
}

public enum NotificationType
{
    System = 1,
    ApprovalRequested = 2,
    ApprovalCompleted = 3,
    ApprovalRejected = 4,
    ApprovalReturned = 5,
    MaintenanceDue = 6,
    ContractExpiring = 7,
    ParkingAllocated = 8
}
