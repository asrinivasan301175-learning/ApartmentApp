using ApartmentMaintenance.Domain.Common;
using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Domain.Entities;

public class CommitteeMember : AuditableEntity<int>
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    public string CommitteeRole { get; set; } = string.Empty; // President, Secretary, Treasurer
    public DateTime TermStartDateUtc { get; set; }
    public DateTime TermEndDateUtc { get; set; }
    public bool IsCurrentActive { get; set; } = true;
    public string? Remarks { get; set; }
}

public class PurchaseRequest : AuditableEntity<Guid>
{
    public PurchaseRequest()
    {
        Id = Guid.NewGuid();
    }

    public string RequestNumber { get; set; } = string.Empty; // e.g., "PR-2026-0001"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid RequesterId { get; set; }
    public User Requester { get; set; } = default!;

    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public decimal EstimatedTotalAmount { get; set; }
    public decimal TaxAndOtherCharges { get; set; }
    public decimal GrandTotal => EstimatedTotalAmount + TaxAndOtherCharges;

    public string BudgetCategory { get; set; } = "General Maintenance"; // e.g., Electrical, Civil, Security, Landscaping
    public DateTime RequiredDeliveryDate { get; set; }
    public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.Draft;
    
    // Approval Sequence Progress: 1 = President, 2 = Secretary, 3 = Treasurer, 4 = Finalized
    public int CurrentApprovalStep { get; set; } = 1;
    public string? CurrentAssignedRole { get; set; } = SystemRoles.President;

    // Optimistic Concurrency Token
    public byte[]? RowVersion { get; set; }

    // Navigation
    public ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();
    public ICollection<PurchaseRequestAttachment> Attachments { get; set; } = new List<PurchaseRequestAttachment>();
    public ICollection<PurchaseApproval> Approvals { get; set; } = new List<PurchaseApproval>();
    public PurchaseOrder? PurchaseOrder { get; set; }
}

public class PurchaseRequestItem : AuditableEntity<int>
{
    public Guid PurchaseRequestId { get; set; }
    public PurchaseRequest PurchaseRequest { get; set; } = default!;

    public string ItemDescription { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string UnitOfMeasure { get; set; } = "Nos"; // Nos, Kg, Meters, Hours, Service
    public decimal EstimatedUnitPrice { get; set; }
    public decimal TotalEstimatedPrice => Quantity * EstimatedUnitPrice;

    public int? RelatedAssetId { get; set; }
    public Asset? RelatedAsset { get; set; }
}

public class PurchaseRequestAttachment : AuditableEntity<int>
{
    public Guid PurchaseRequestId { get; set; }
    public PurchaseRequest PurchaseRequest { get; set; } = default!;

    public string FileName { get; set; } = string.Empty;
    public string StoredFilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string? Description { get; set; } // Quotation, Technical Specification, Inspection Report
}

public class PurchaseApproval : AuditableEntity<int>
{
    public Guid PurchaseRequestId { get; set; }
    public PurchaseRequest PurchaseRequest { get; set; } = default!;

    public Guid ApproverId { get; set; }
    public User Approver { get; set; } = default!;

    public string ApproverRole { get; set; } = string.Empty; // President, Secretary, Treasurer
    public int StepOrder { get; set; } // 1, 2, 3
    public ApprovalDecision Decision { get; set; }
    public string Comments { get; set; } = string.Empty;
    public DateTime DecisionDateUtc { get; set; } = DateTime.UtcNow;
}

public class PurchaseOrder : AuditableEntity<Guid>
{
    public PurchaseOrder()
    {
        Id = Guid.NewGuid();
    }

    public string PoNumber { get; set; } = string.Empty; // e.g., "PO-2026-0001"
    public Guid PurchaseRequestId { get; set; }
    public PurchaseRequest PurchaseRequest { get; set; } = default!;

    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = default!;

    public DateTime OrderDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal GrandTotal => TotalAmount + TaxAmount;

    public PurchaseOrderStatus DeliveryStatus { get; set; } = PurchaseOrderStatus.Issued;
    public string? TermsAndConditions { get; set; }
    public string? DeliveryNotes { get; set; }

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}

public class PurchaseOrderItem : AuditableEntity<int>
{
    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = default!;

    public string ItemDescription { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public string UnitOfMeasure { get; set; } = "Nos";
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
