using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApartmentMaintenance.Application.Common;
using ApartmentMaintenance.Application.DTOs;
using ApartmentMaintenance.Application.Interfaces;
using ApartmentMaintenance.Domain.Entities;
using ApartmentMaintenance.Domain.Enums;

namespace ApartmentMaintenance.Api.Controllers;

[Authorize]
public class EmployeesController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;

    public EmployeesController(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<EmployeeDto>>>> GetEmployees(
        [FromQuery] string? search,
        [FromQuery] string? department,
        [FromQuery] EmploymentStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var canViewSalary = _currentUserService.IsInRole(SystemRoles.SystemAdmin) ||
                            _currentUserService.IsInRole(SystemRoles.President) ||
                            _currentUserService.IsInRole(SystemRoles.Treasurer) ||
                            _currentUserService.IsInRole(SystemRoles.Secretary);

        var query = _context.Employees.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            query = query.Where(e => e.Department == department.Trim());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(e => e.EmployeeCode.ToLower().Contains(searchLower) ||
                                     e.FirstName.ToLower().Contains(searchLower) ||
                                     e.LastName.ToLower().Contains(searchLower) ||
                                     e.MobileNumber.Contains(searchLower));
        }

        var totalCount = await query.CountAsync();
        var employees = await query
            .OrderBy(e => e.EmployeeCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                UserId = e.UserId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                FullName = e.FullName,
                Designation = e.Designation,
                Department = e.Department,
                MobileNumber = e.MobileNumber,
                Email = e.Email,
                Address = e.Address,
                JoiningDate = e.JoiningDate,
                EmploymentType = e.EmploymentType.ToString(),
                MonthlySalary = canViewSalary ? e.MonthlySalary : null,
                EmergencyContactName = e.EmergencyContactName,
                EmergencyContactNumber = e.EmergencyContactNumber,
                Status = e.Status.ToString(),
                IsActive = e.IsActive
            })
            .ToListAsync();

        return OkResponse(new PagedResult<EmployeeDto>(employees, totalCount, page, pageSize));
    }

    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary}")]
    public async Task<ActionResult<ApiResponse<EmployeeDto>>> CreateEmployee([FromBody] CreateEmployeeDto request)
    {
        if (await _context.Employees.AnyAsync(e => e.EmployeeCode == request.EmployeeCode.Trim()))
        {
            return FailResponse<EmployeeDto>("An employee with this Employee Code already exists.");
        }

        var employee = new Employee
        {
            EmployeeCode = request.EmployeeCode.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Designation = request.Designation.Trim(),
            Department = request.Department.Trim(),
            MobileNumber = request.MobileNumber.Trim(),
            Email = request.Email.Trim(),
            Address = request.Address.Trim(),
            JoiningDate = request.JoiningDate,
            EmploymentType = request.EmploymentType,
            MonthlySalary = request.MonthlySalary,
            EmergencyContactName = request.EmergencyContactName?.Trim(),
            EmergencyContactNumber = request.EmergencyContactNumber?.Trim(),
            Status = EmploymentStatus.Active,
            IsActive = true
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("Employee", employee.Id.ToString(), "Create", null, $"Registered employee {employee.FullName} ({employee.EmployeeCode})");

        return OkResponse(new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = employee.FullName,
            Designation = employee.Designation,
            Department = employee.Department,
            MobileNumber = employee.MobileNumber,
            Email = employee.Email,
            JoiningDate = employee.JoiningDate,
            MonthlySalary = employee.MonthlySalary,
            Status = employee.Status.ToString()
        }, "Employee created successfully.");
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary}")]
    public async Task<ActionResult<ApiResponse<string>>> ToggleStatus(int id, [FromBody] EmploymentStatus newStatus)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee == null) return FailResponse<string>("Employee not found.", statusCode: 404);

        employee.Status = newStatus;
        employee.IsActive = newStatus == EmploymentStatus.Active || newStatus == EmploymentStatus.Probation;
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync("Employee", employee.Id.ToString(), "StatusChange", null, $"Employee status updated to {newStatus}");
        return OkResponse($"Employee status updated to {newStatus}.");
    }
}

[Authorize]
public class VendorsController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public VendorsController(IApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<List<VendorCategory>>>> GetCategories()
    {
        var categories = await _context.VendorCategories.OrderBy(c => c.Name).ToListAsync();
        return OkResponse(categories);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<VendorDto>>>> GetVendors(
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Vendors
            .Include(v => v.Category)
            .Include(v => v.Contracts)
            .AsNoTracking()
            .AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(v => v.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(v => v.VendorName.ToLower().Contains(searchLower) ||
                                     v.ContactPerson.ToLower().Contains(searchLower) ||
                                     v.MobileNumber.Contains(searchLower));
        }

        var totalCount = await query.CountAsync();
        var vendors = await query
            .OrderBy(v => v.VendorName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VendorDto
            {
                Id = v.Id,
                VendorName = v.VendorName,
                CategoryId = v.CategoryId,
                CategoryName = v.Category.Name,
                ContactPerson = v.ContactPerson,
                MobileNumber = v.MobileNumber,
                Email = v.Email,
                Address = v.Address,
                TaxRegistrationNumber = v.TaxRegistrationNumber,
                PaymentTerms = v.PaymentTerms,
                ServiceStatus = v.ServiceStatus,
                IsActive = v.IsActive,
                ActiveContractsCount = v.Contracts.Count(c => c.Status == ContractStatus.Active),
                NextContractExpiryDate = v.Contracts.Where(c => c.Status == ContractStatus.Active && c.EndDate >= DateTime.UtcNow)
                    .OrderBy(c => c.EndDate)
                    .Select(c => (DateTime?)c.EndDate)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return OkResponse(new PagedResult<VendorDto>(vendors, totalCount, page, pageSize));
    }

    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary}")]
    public async Task<ActionResult<ApiResponse<VendorDto>>> CreateVendor([FromBody] CreateVendorDto request)
    {
        var vendor = new Vendor
        {
            VendorName = request.VendorName.Trim(),
            CategoryId = request.CategoryId,
            ContactPerson = request.ContactPerson.Trim(),
            MobileNumber = request.MobileNumber.Trim(),
            Email = request.Email.Trim(),
            Address = request.Address.Trim(),
            TaxRegistrationNumber = request.TaxRegistrationNumber?.Trim(),
            PaymentTerms = request.PaymentTerms?.Trim(),
            ServiceStatus = "Active",
            IsActive = true
        };

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("Vendor", vendor.Id.ToString(), "Create", null, $"Added vendor {vendor.VendorName}");

        return OkResponse(new VendorDto
        {
            Id = vendor.Id,
            VendorName = vendor.VendorName,
            CategoryId = vendor.CategoryId,
            ContactPerson = vendor.ContactPerson,
            MobileNumber = vendor.MobileNumber,
            Email = vendor.Email,
            ServiceStatus = vendor.ServiceStatus
        }, "Vendor profile created successfully.");
    }

    [HttpGet("{id}/contracts")]
    public async Task<ActionResult<ApiResponse<List<VendorContractDto>>>> GetContracts(int id)
    {
        var contracts = await _context.VendorContracts
            .Where(vc => vc.VendorId == id)
            .OrderByDescending(vc => vc.StartDate)
            .Select(vc => new VendorContractDto
            {
                Id = vc.Id,
                VendorId = vc.VendorId,
                VendorName = vc.Vendor.VendorName,
                ContractTitle = vc.ContractTitle,
                ContractNumber = vc.ContractNumber,
                StartDate = vc.StartDate,
                EndDate = vc.EndDate,
                RenewalReminderDate = vc.RenewalReminderDate,
                ContractValue = vc.ContractValue,
                Status = vc.Status.ToString(),
                ScopeOfWork = vc.ScopeOfWork
            })
            .ToListAsync();

        return OkResponse(contracts);
    }

    [HttpPost("{id}/contracts")]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary}")]
    public async Task<ActionResult<ApiResponse<VendorContractDto>>> AddContract(int id, [FromBody] CreateVendorContractDto request)
    {
        var contract = new VendorContract
        {
            VendorId = id,
            ContractTitle = request.ContractTitle.Trim(),
            ContractNumber = request.ContractNumber.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RenewalReminderDate = request.RenewalReminderDate ?? request.EndDate.AddDays(-30),
            ContractValue = request.ContractValue,
            ScopeOfWork = request.ScopeOfWork?.Trim(),
            Status = ContractStatus.Active,
            IsActive = true
        };

        _context.VendorContracts.Add(contract);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("VendorContract", contract.Id.ToString(), "Create", null, $"Created contract {contract.ContractNumber} for vendor {id}");

        return OkResponse(new VendorContractDto
        {
            Id = contract.Id,
            VendorId = contract.VendorId,
            ContractTitle = contract.ContractTitle,
            ContractNumber = contract.ContractNumber,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            ContractValue = contract.ContractValue,
            Status = contract.Status.ToString()
        }, "Contract created.");
    }
}
