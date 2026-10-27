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
public class TowersController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public TowersController(IApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TowerDto>>>> GetTowers()
    {
        var towers = await _context.Towers
            .Include(t => t.Flats)
            .AsNoTracking()
            .OrderBy(t => t.TowerCode)
            .Select(t => new TowerDto
            {
                Id = t.Id,
                TowerCode = t.TowerCode,
                TowerName = t.TowerName,
                NumberOfFloors = t.NumberOfFloors,
                TotalFlats = t.TotalFlats,
                Description = t.Description,
                IsActive = t.IsActive,
                OccupiedFlatsCount = t.Flats.Count(f => f.OccupancyStatus == FlatOccupancyStatus.SelfOccupied || f.OccupancyStatus == FlatOccupancyStatus.TenantOccupied),
                VacantFlatsCount = t.Flats.Count(f => f.OccupancyStatus == FlatOccupancyStatus.Vacant)
            })
            .ToListAsync();

        return OkResponse(towers);
    }

    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary}")]
    public async Task<ActionResult<ApiResponse<TowerDto>>> CreateTower([FromBody] CreateTowerDto request)
    {
        if (await _context.Towers.AnyAsync(t => t.TowerCode == request.TowerCode.Trim()))
        {
            return FailResponse<TowerDto>("Tower with this code already exists.");
        }

        var tower = new Tower
        {
            TowerCode = request.TowerCode.Trim(),
            TowerName = request.TowerName.Trim(),
            NumberOfFloors = request.NumberOfFloors,
            TotalFlats = request.TotalFlats,
            Description = request.Description?.Trim(),
            IsActive = true
        };

        _context.Towers.Add(tower);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("Tower", tower.Id.ToString(), "Create", null, $"Created tower {tower.TowerCode}");

        return OkResponse(new TowerDto
        {
            Id = tower.Id,
            TowerCode = tower.TowerCode,
            TowerName = tower.TowerName,
            NumberOfFloors = tower.NumberOfFloors,
            TotalFlats = tower.TotalFlats,
            Description = tower.Description,
            IsActive = tower.IsActive
        }, "Tower created successfully.");
    }
}

[Authorize]
public class FlatsController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public FlatsController(IApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<FlatDto>>>> GetFlats(
        [FromQuery] int? towerId,
        [FromQuery] string? search,
        [FromQuery] FlatOccupancyStatus? occupancyStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Flats
            .Include(f => f.Tower)
            .Include(f => f.Ownerships).ThenInclude(o => o.Resident)
            .Include(f => f.Occupancies).ThenInclude(o => o.Resident)
            .Include(f => f.ParkingAllocations).ThenInclude(pa => pa.ParkingLot)
            .AsNoTracking()
            .AsQueryable();

        if (towerId.HasValue)
        {
            query = query.Where(f => f.TowerId == towerId.Value);
        }

        if (occupancyStatus.HasValue)
        {
            query = query.Where(f => f.OccupancyStatus == occupancyStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(f => f.FlatNumber.ToLower().Contains(searchLower) ||
                                     f.Tower.TowerName.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync();
        var flats = await query
            .OrderBy(f => f.Tower.TowerCode)
            .ThenBy(f => f.FloorNumber)
            .ThenBy(f => f.FlatNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FlatDto
            {
                Id = f.Id,
                TowerId = f.TowerId,
                TowerCode = f.Tower.TowerCode,
                TowerName = f.Tower.TowerName,
                FlatNumber = f.FlatNumber,
                FloorNumber = f.FloorNumber,
                AreaSqFt = f.AreaSqFt,
                AreaMeasurementType = f.AreaMeasurementType.ToString(),
                OwnershipStatus = f.OwnershipStatus.ToString(),
                OccupancyStatus = f.OccupancyStatus.ToString(),
                IsActive = f.IsActive,
                Owners = f.Ownerships.Where(o => o.IsActive).Select(o => new FlatOwnerSummaryDto
                {
                    ResidentId = o.ResidentId,
                    FullName = o.Resident.FullName,
                    Email = o.Resident.Email,
                    MobileNumber = o.Resident.MobileNumber,
                    OwnershipPercentage = o.OwnershipPercentage,
                    IsPrimaryOwner = o.IsPrimaryOwner,
                    StartDateUtc = o.StartDateUtc
                }).ToList(),
                Occupants = f.Occupancies.Where(o => o.IsActive).Select(o => new FlatOccupantSummaryDto
                {
                    ResidentId = o.ResidentId,
                    FullName = o.Resident.FullName,
                    MobileNumber = o.Resident.MobileNumber,
                    OccupancyType = o.OccupancyType.ToString(),
                    MoveInDateUtc = o.MoveInDateUtc
                }).ToList(),
                AllocatedParkingLots = f.ParkingAllocations.Where(pa => pa.IsActive).Select(pa => pa.ParkingLot.LotNumber).ToList()
            })
            .ToListAsync();

        return OkResponse(new PagedResult<FlatDto>(flats, totalCount, page, pageSize));
    }

    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary}")]
    public async Task<ActionResult<ApiResponse<FlatDto>>> CreateFlat([FromBody] CreateFlatDto request)
    {
        if (await _context.Flats.AnyAsync(f => f.TowerId == request.TowerId && f.FlatNumber == request.FlatNumber.Trim()))
        {
            return FailResponse<FlatDto>("Flat number already exists in this tower.");
        }

        var flat = new Flat
        {
            TowerId = request.TowerId,
            FlatNumber = request.FlatNumber.Trim(),
            FloorNumber = request.FloorNumber,
            AreaSqFt = request.AreaSqFt,
            AreaMeasurementType = request.AreaMeasurementType,
            OwnershipStatus = request.OwnershipStatus,
            OccupancyStatus = request.OccupancyStatus,
            IsActive = true
        };

        _context.Flats.Add(flat);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("Flat", flat.Id.ToString(), "Create", null, $"Created flat {flat.FlatNumber} in tower {flat.TowerId}");

        return OkResponse(new FlatDto
        {
            Id = flat.Id,
            TowerId = flat.TowerId,
            FlatNumber = flat.FlatNumber,
            FloorNumber = flat.FloorNumber,
            AreaSqFt = flat.AreaSqFt,
            AreaMeasurementType = flat.AreaMeasurementType.ToString(),
            OwnershipStatus = flat.OwnershipStatus.ToString(),
            OccupancyStatus = flat.OccupancyStatus.ToString(),
            IsActive = flat.IsActive
        }, "Flat created successfully.");
    }

    [HttpPost("ownership")]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary}")]
    public async Task<ActionResult<ApiResponse<string>>> AssignOwnership([FromBody] AssignOwnershipDto request)
    {
        var flat = await _context.Flats.Include(f => f.Ownerships).FirstOrDefaultAsync(f => f.Id == request.FlatId);
        if (flat == null) return FailResponse<string>("Flat not found.");

        var resident = await _context.Residents.FirstOrDefaultAsync(r => r.Id == request.ResidentId);
        if (resident == null) return FailResponse<string>("Resident not found.");

        var ownership = new FlatOwnership
        {
            FlatId = request.FlatId,
            ResidentId = request.ResidentId,
            OwnershipPercentage = request.OwnershipPercentage,
            IsPrimaryOwner = request.IsPrimaryOwner,
            StartDateUtc = DateTime.UtcNow,
            IsActive = true
        };

        flat.OwnershipStatus = FlatOwnershipStatus.Sold;
        _context.FlatOwnerships.Add(ownership);
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync("FlatOwnership", ownership.Id.ToString(), "Assign", null, $"Assigned ownership of flat {flat.FlatNumber} to resident {resident.FullName}");

        return OkResponse("Ownership assigned successfully.");
    }
}

[Authorize]
public class ResidentsController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public ResidentsController(IApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ResidentDto>>>> GetResidents(
        [FromQuery] string? search,
        [FromQuery] ResidentType? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Residents
            .Include(r => r.FlatOwnerships).ThenInclude(o => o.Flat).ThenInclude(f => f.Tower)
            .Include(r => r.FlatOccupancies).ThenInclude(o => o.Flat).ThenInclude(f => f.Tower)
            .AsNoTracking()
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(r => r.ResidentType == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(r => r.FullName.ToLower().Contains(searchLower) ||
                                     r.Email.ToLower().Contains(searchLower) ||
                                     r.MobileNumber.Contains(searchLower));
        }

        var totalCount = await query.CountAsync();
        var residents = await query
            .OrderBy(r => r.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ResidentDto
            {
                Id = r.Id,
                UserId = r.UserId,
                FullName = r.FullName,
                Email = r.Email,
                MobileNumber = r.MobileNumber,
                ResidentType = r.ResidentType.ToString(),
                EmergencyContactName = r.EmergencyContactName,
                EmergencyContactNumber = r.EmergencyContactNumber,
                CorrespondenceAddress = r.CorrespondenceAddress,
                Status = r.Status,
                AssociatedFlats = r.FlatOwnerships.Where(o => o.IsActive).Select(o => $"{o.Flat.Tower.TowerCode}-{o.Flat.FlatNumber} (Owner)")
                    .Concat(r.FlatOccupancies.Where(o => o.IsActive).Select(o => $"{o.Flat.Tower.TowerCode}-{o.Flat.FlatNumber} (Tenant)"))
                    .ToList()
            })
            .ToListAsync();

        return OkResponse(new PagedResult<ResidentDto>(residents, totalCount, page, pageSize));
    }

    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary}")]
    public async Task<ActionResult<ApiResponse<ResidentDto>>> CreateResident([FromBody] CreateResidentDto request)
    {
        var resident = new Resident
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            MobileNumber = request.MobileNumber.Trim(),
            ResidentType = request.ResidentType,
            EmergencyContactName = request.EmergencyContactName?.Trim(),
            EmergencyContactNumber = request.EmergencyContactNumber?.Trim(),
            CorrespondenceAddress = request.CorrespondenceAddress?.Trim(),
            Status = "Active"
        };

        _context.Residents.Add(resident);
        await _context.SaveChangesAsync();

        if (request.InitialFlatId.HasValue)
        {
            if (request.ResidentType == ResidentType.Owner)
            {
                _context.FlatOwnerships.Add(new FlatOwnership
                {
                    FlatId = request.InitialFlatId.Value,
                    ResidentId = resident.Id,
                    OwnershipPercentage = 100m,
                    IsPrimaryOwner = true
                });
            }
            else
            {
                _context.FlatOccupancies.Add(new FlatOccupancy
                {
                    FlatId = request.InitialFlatId.Value,
                    ResidentId = resident.Id,
                    OccupancyType = request.ResidentType
                });
            }
            await _context.SaveChangesAsync();
        }

        await _auditLogService.LogAsync("Resident", resident.Id.ToString(), "Create", null, $"Created resident {resident.FullName}");

        return OkResponse(new ResidentDto
        {
            Id = resident.Id,
            FullName = resident.FullName,
            Email = resident.Email,
            MobileNumber = resident.MobileNumber,
            ResidentType = resident.ResidentType.ToString(),
            Status = resident.Status
        }, "Resident registered successfully.");
    }
}
