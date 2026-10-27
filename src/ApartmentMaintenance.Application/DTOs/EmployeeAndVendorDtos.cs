using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Application.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime JoiningDate { get; set; }
    public string EmploymentType { get; set; } = string.Empty;
    public decimal? MonthlySalary { get; set; } // Masked if user lacks payroll privilege
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateEmployeeDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
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
}

public class VendorDto
{
    public int Id { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? TaxRegistrationNumber { get; set; }
    public string? PaymentTerms { get; set; }
    public string ServiceStatus { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ActiveContractsCount { get; set; }
    public DateTime? NextContractExpiryDate { get; set; }
}

public class CreateVendorDto
{
    public string VendorName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string ContactPerson { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? TaxRegistrationNumber { get; set; }
    public string? PaymentTerms { get; set; }
}

public class VendorContractDto
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string ContractTitle { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalReminderDate { get; set; }
    public decimal ContractValue { get; set; }
    public string? DocumentUrl { get; set; }
    public string? DocumentFileName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ScopeOfWork { get; set; }
}

public class CreateVendorContractDto
{
    public int VendorId { get; set; }
    public string ContractTitle { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? RenewalReminderDate { get; set; }
    public decimal ContractValue { get; set; }
    public string? ScopeOfWork { get; set; }
}
