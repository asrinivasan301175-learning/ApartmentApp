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
public class CommitteeController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public CommitteeController(IApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CommitteeMemberDto>>>> GetCommitteeMembers([FromQuery] bool activeOnly = true)
    {
        var query = _context.CommitteeMembers
            .Include(c => c.User)
            .AsNoTracking()
            .AsQueryable();

        if (activeOnly)
        {
            query = query.Where(c => c.IsCurrentActive);
        }

        var list = await query
            .OrderBy(c => c.CommitteeRole)
            .Select(c => new CommitteeMemberDto
            {
                Id = c.Id,
                UserId = c.UserId,
                MemberName = c.User.FullName,
                MemberEmail = c.User.Email,
                MemberPhone = c.User.PhoneNumber,
                CommitteeRole = c.CommitteeRole,
                TermStartDateUtc = c.TermStartDateUtc,
                TermEndDateUtc = c.TermEndDateUtc,
                IsCurrentActive = c.IsCurrentActive,
                Remarks = c.Remarks
            })
            .ToListAsync();

        return OkResponse(list);
    }

    [HttpPost("assign")]
    [Authorize(Roles = SystemRoles.SystemAdmin)]
    public async Task<ActionResult<ApiResponse<string>>> AssignCommitteeRole([FromBody] AssignCommitteeMemberDto request)
    {
        var user = await _context.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == request.UserId);
        if (user == null) return FailResponse<string>("User not found.", statusCode: 404);

        // Deactivate previous active member in this role
        var existing = await _context.CommitteeMembers
            .Where(c => c.CommitteeRole == request.CommitteeRole && c.IsCurrentActive)
            .ToListAsync();

        foreach (var m in existing)
        {
            m.IsCurrentActive = false;
        }

        var newMember = new CommitteeMember
        {
            UserId = request.UserId,
            CommitteeRole = request.CommitteeRole,
            TermStartDateUtc = request.TermStartDateUtc,
            TermEndDateUtc = request.TermEndDateUtc,
            IsCurrentActive = true,
            Remarks = request.Remarks?.Trim()
        };

        // Ensure user has this role mapping in UserRoles
        var targetRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == request.CommitteeRole);
        if (targetRole != null && !user.UserRoles.Any(ur => ur.RoleId == targetRole.Id))
        {
            user.UserRoles.Add(new UserRoleMapping { UserId = user.Id, RoleId = targetRole.Id });
        }

        _context.CommitteeMembers.Add(newMember);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("CommitteeMember", newMember.Id.ToString(), "Assign", null, $"Assigned {user.FullName} as {request.CommitteeRole}");

        return OkResponse($"Assigned {user.FullName} as {request.CommitteeRole}.");
    }
}

[Authorize]
public class DashboardController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DashboardController(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<ApiResponse<DashboardMetricsDto>>> GetMetrics()
    {
        var totalTowers = await _context.Towers.CountAsync();
        var totalFlats = await _context.Flats.CountAsync();
        var occupiedFlats = await _context.Flats.CountAsync(f => f.OccupancyStatus == FlatOccupancyStatus.SelfOccupied || f.OccupancyStatus == FlatOccupancyStatus.TenantOccupied);
        var vacantFlats = await _context.Flats.CountAsync(f => f.OccupancyStatus == FlatOccupancyStatus.Vacant);

        var totalParking = await _context.ParkingLots.CountAsync();
        var allocatedParking = await _context.ParkingAllocations.CountAsync(pa => pa.IsActive);
        var availableParking = totalParking - allocatedParking;

        var totalResidents = await _context.Residents.CountAsync();
        var totalEmployees = await _context.Employees.CountAsync(e => e.Status == EmploymentStatus.Active);
        var activeVendors = await _context.Vendors.CountAsync(v => v.ServiceStatus == "Active");

        var thirtyDaysAhead = DateTime.UtcNow.AddDays(30);
        var expiringContracts = await _context.VendorContracts.CountAsync(c => c.Status == ContractStatus.Active && c.EndDate <= thirtyDaysAhead);

        var totalAssets = await _context.Assets.CountAsync();
        var assetsDueMaintenance = await _context.Assets.CountAsync(a => a.NextScheduledMaintenanceDate <= thirtyDaysAhead || a.Condition == AssetCondition.NeedsMaintenance);

        var pendingPurchaseRequests = await _context.PurchaseRequests.CountAsync(p => p.Status == PurchaseRequestStatus.PendingApproval);

        // My pending approvals calculation
        var userRoles = _currentUserService.Roles;
        var myPendingApprovalsCount = await _context.PurchaseRequests.CountAsync(pr =>
            pr.Status == PurchaseRequestStatus.PendingApproval &&
            ((pr.CurrentApprovalStep == 1 && userRoles.Contains(SystemRoles.President)) ||
             (pr.CurrentApprovalStep == 2 && userRoles.Contains(SystemRoles.Secretary)) ||
             (pr.CurrentApprovalStep == 3 && userRoles.Contains(SystemRoles.Treasurer)) ||
             userRoles.Contains(SystemRoles.SystemAdmin)));

        var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var approvedOrdersThisMonth = await _context.PurchaseOrders.CountAsync(po => po.OrderDateUtc >= firstDayOfMonth);
        var spendThisMonth = await _context.PurchaseOrders
            .Where(po => po.OrderDateUtc >= firstDayOfMonth)
            .SumAsync(po => po.TotalAmount + po.TaxAmount);

        var recentAudits = await _context.AuditLogs
            .OrderByDescending(a => a.TimestampUtc)
            .Take(6)
            .Select(a => new RecentActivityDto
            {
                Title = $"{a.Action} on {a.EntityName}",
                Description = a.NewValues ?? $"{a.EntityName} ID: {a.EntityId}",
                TimestampUtc = a.TimestampUtc,
                Type = a.Action
            })
            .ToListAsync();

        var metrics = new DashboardMetricsDto
        {
            TotalTowers = totalTowers,
            TotalFlats = totalFlats,
            OccupiedFlats = occupiedFlats,
            VacantFlats = vacantFlats,
            TotalParkingLots = totalParking,
            AllocatedParkingLots = allocatedParking,
            AvailableParkingLots = Math.Max(0, availableParking),
            TotalResidents = totalResidents,
            TotalEmployees = totalEmployees,
            ActiveVendors = activeVendors,
            ContractsExpiringIn30Days = expiringContracts,
            TotalAssets = totalAssets,
            AssetsDueForMaintenance = assetsDueMaintenance,
            PendingPurchaseRequests = pendingPurchaseRequests,
            MyPendingApprovalsCount = myPendingApprovalsCount,
            ApprovedPurchaseOrdersThisMonth = approvedOrdersThisMonth,
            TotalApprovedSpendThisMonth = spendThisMonth,
            RecentActivities = recentAudits
        };

        return OkResponse(metrics);
    }
}

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class AuditLogsController : BaseApiController
{
    private readonly IApplicationDbContext _context;

    public AuditLogsController(IApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLog>>>> GetLogs(
        [FromQuery] string? entityName,
        [FromQuery] string? action,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(a => a.EntityName == entityName.Trim());
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action == action.Trim());
        }

        var totalCount = await query.CountAsync();
        var logs = await query
            .OrderByDescending(a => a.TimestampUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return OkResponse(new PagedResult<AuditLog>(logs, totalCount, page, pageSize));
    }
}
