using ApartmentMaintenance.Domain.Common;
using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Domain.Entities;

public class Employee : AuditableEntity<int>
{
    public string EmployeeCode { get; set; } = string.Empty; // e.g. "EMP-001"
    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Designation { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime JoiningDate { get; set; }
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    public decimal MonthlySalary { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
}

public class VendorCategory : AuditableEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public VendorCategoryType CategoryType { get; set; } = VendorCategoryType.Other;

    public ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
}

public class Vendor : AuditableEntity<int>
{
    public string VendorName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public VendorCategory Category { get; set; } = default!;

    public string ContactPerson { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? TaxRegistrationNumber { get; set; } // GST / PAN
    public string? PaymentTerms { get; set; } // e.g. "Net 30 days"
    public string ServiceStatus { get; set; } = "Active";

    // Navigation
    public ICollection<VendorContract> Contracts { get; set; } = new List<VendorContract>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
    public ICollection<PurchaseRequest> PurchaseRequests { get; set; } = new List<PurchaseRequest>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}

public class VendorContract : AuditableEntity<int>
{
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = default!;

    public string ContractTitle { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalReminderDate { get; set; }
    public decimal ContractValue { get; set; }
    public string? DocumentUrl { get; set; }
    public string? DocumentFileName { get; set; }
    public ContractStatus Status { get; set; } = ContractStatus.Active;
    public string? ScopeOfWork { get; set; }
}
