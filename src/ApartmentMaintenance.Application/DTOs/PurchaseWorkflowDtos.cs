using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Application.DTOs;

public class PurchaseRequestDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid RequesterId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public int? VendorId { get; set; }
    public string? VendorName { get; set; }
    public decimal EstimatedTotalAmount { get; set; }
    public decimal TaxAndOtherCharges { get; set; }
    public decimal GrandTotal { get; set; }
    public string BudgetCategory { get; set; } = string.Empty;
    public DateTime RequiredDeliveryDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CurrentApprovalStep { get; set; }
    public string? CurrentAssignedRole { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public byte[]? RowVersion { get; set; }

    public List<PurchaseRequestItemDto> Items { get; set; } = new();
    public List<PurchaseApprovalDto> Approvals { get; set; } = new();
    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
}

public class PurchaseRequestItemDto
{
    public int Id { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal EstimatedUnitPrice { get; set; }
    public decimal TotalEstimatedPrice { get; set; }
    public int? RelatedAssetId { get; set; }
    public string? RelatedAssetName { get; set; }
}

public class CreatePurchaseRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? VendorId { get; set; }
    public string BudgetCategory { get; set; } = "General Maintenance";
    public DateTime RequiredDeliveryDate { get; set; }
    public decimal TaxAndOtherCharges { get; set; }
    public List<CreatePurchaseRequestItemDto> Items { get; set; } = new();
    public bool SubmitImmediately { get; set; } = true;
}

public class CreatePurchaseRequestItemDto
{
    public string ItemDescription { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string UnitOfMeasure { get; set; } = "Nos";
    public decimal EstimatedUnitPrice { get; set; }
    public int? RelatedAssetId { get; set; }
}

public class PurchaseApprovalActionDto
{
    public ApprovalDecision Decision { get; set; } // Approved = 1, Rejected = 2, ReturnedForChanges = 3
    public string Comments { get; set; } = string.Empty;
    public byte[]? RowVersion { get; set; }
}

public class PurchaseApprovalDto
{
    public int Id { get; set; }
    public Guid ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string ApproverRole { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public DateTime DecisionDateUtc { get; set; }
}

public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public string PoNumber { get; set; } = string.Empty;
    public Guid PurchaseRequestId { get; set; }
    public string PurchaseRequestNumber { get; set; } = string.Empty;
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string DeliveryStatus { get; set; } = string.Empty;
    public string? TermsAndConditions { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public string ItemDescription { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
