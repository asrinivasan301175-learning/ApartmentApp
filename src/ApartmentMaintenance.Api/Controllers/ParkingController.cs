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
public class ParkingController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public ParkingController(IApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpGet("lots")]
    public async Task<ActionResult<ApiResponse<List<ParkingLotDto>>>> GetParkingLots(
        [FromQuery] int? towerId,
        [FromQuery] ParkingLotStatus? status,
        [FromQuery] string? search)
    {
        var query = _context.ParkingLots
            .Include(p => p.Tower)
            .Include(p => p.Allocations.Where(a => a.IsActive))
                .ThenInclude(a => a.Flat)
            .Include(p => p.Allocations.Where(a => a.IsActive))
                .ThenInclude(a => a.Resident)
            .AsNoTracking()
            .AsQueryable();

        if (towerId.HasValue)
        {
            query = query.Where(p => p.TowerId == towerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(p => p.LotNumber.ToLower().Contains(searchLower) ||
                                     (p.Tower != null && p.Tower.TowerName.ToLower().Contains(searchLower)));
        }

        var lots = await query
            .OrderBy(p => p.LotNumber)
            .Select(p => new ParkingLotDto
            {
                Id = p.Id,
                LotNumber = p.LotNumber,
                TowerId = p.TowerId,
                TowerName = p.Tower != null ? p.Tower.TowerName : "General / Open",
                ParkingType = p.ParkingType.ToString(),
                FloorOrBasementLevel = p.FloorOrBasementLevel,
                Status = p.Status.ToString(),
                Remarks = p.Remarks,
                IsActive = p.IsActive,
                AllocatedFlatId = p.Allocations.FirstOrDefault(a => a.IsActive) != null ? p.Allocations.First(a => a.IsActive).FlatId : null,
                AllocatedFlatNumber = p.Allocations.FirstOrDefault(a => a.IsActive) != null ? p.Allocations.First(a => a.IsActive).Flat.FlatNumber : null,
                AllocatedResidentName = p.Allocations.FirstOrDefault(a => a.IsActive) != null && p.Allocations.First(a => a.IsActive).Resident != null ? p.Allocations.First(a => a.IsActive).Resident!.FullName : null,
                VehicleNumber = p.Allocations.FirstOrDefault(a => a.IsActive) != null ? p.Allocations.First(a => a.IsActive).VehicleNumber : null,
                AllocationStartDateUtc = p.Allocations.FirstOrDefault(a => a.IsActive) != null ? p.Allocations.First(a => a.IsActive).StartDateUtc : null
            })
            .ToListAsync();

        return OkResponse(lots);
    }

    [HttpPost("lots")]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary}")]
    public async Task<ActionResult<ApiResponse<ParkingLotDto>>> CreateParkingLot([FromBody] CreateParkingLotDto request)
    {
        if (await _context.ParkingLots.AnyAsync(p => p.LotNumber == request.LotNumber.Trim()))
        {
            return FailResponse<ParkingLotDto>("A parking lot with this number already exists.");
        }

        var lot = new ParkingLot
        {
            LotNumber = request.LotNumber.Trim(),
            TowerId = request.TowerId,
            ParkingType = request.ParkingType,
            FloorOrBasementLevel = request.FloorOrBasementLevel.Trim(),
            Remarks = request.Remarks?.Trim(),
            Status = ParkingLotStatus.Available,
            IsActive = true
        };

        _context.ParkingLots.Add(lot);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("ParkingLot", lot.Id.ToString(), "Create", null, $"Created parking lot {lot.LotNumber}");

        return OkResponse(new ParkingLotDto
        {
            Id = lot.Id,
            LotNumber = lot.LotNumber,
            TowerId = lot.TowerId,
            ParkingType = lot.ParkingType.ToString(),
            FloorOrBasementLevel = lot.FloorOrBasementLevel,
            Status = lot.Status.ToString(),
            IsActive = lot.IsActive
        }, "Parking lot registered.");
    }

    [HttpPost("allocate")]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary}")]
    public async Task<ActionResult<ApiResponse<string>>> AllocateParking([FromBody] AllocateParkingDto request)
    {
        var lot = await _context.ParkingLots.Include(p => p.Allocations).FirstOrDefaultAsync(p => p.Id == request.ParkingLotId);
        if (lot == null) return FailResponse<string>("Parking lot not found.");

        if (lot.Allocations.Any(a => a.IsActive))
        {
            return FailResponse<string>("Parking lot is already actively allocated. Please release it first.");
        }

        var flat = await _context.Flats.FirstOrDefaultAsync(f => f.Id == request.FlatId);
        if (flat == null) return FailResponse<string>("Flat not found.");

        var allocation = new ParkingAllocation
        {
            ParkingLotId = request.ParkingLotId,
            FlatId = request.FlatId,
            ResidentId = request.ResidentId,
            VehicleNumber = request.VehicleNumber?.Trim(),
            VehicleType = request.VehicleType?.Trim(),
            StartDateUtc = DateTime.UtcNow,
            IsActive = true
        };

        lot.Status = ParkingLotStatus.Allocated;
        _context.ParkingAllocations.Add(allocation);
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync("ParkingAllocation", allocation.Id.ToString(), "Allocate", null, $"Allocated parking {lot.LotNumber} to flat {flat.FlatNumber}");

        return OkResponse("Parking lot successfully allocated.");
    }

    [HttpPost("release/{allocationId}")]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary}")]
    public async Task<ActionResult<ApiResponse<string>>> ReleaseParking(int allocationId)
    {
        var allocation = await _context.ParkingAllocations.Include(a => a.ParkingLot).FirstOrDefaultAsync(a => a.Id == allocationId);
        if (allocation == null || !allocation.IsActive)
        {
            return FailResponse<string>("Active parking allocation not found.");
        }

        allocation.IsActive = false;
        allocation.EndDateUtc = DateTime.UtcNow;

        if (allocation.ParkingLot != null)
        {
            allocation.ParkingLot.Status = ParkingLotStatus.Available;
        }

        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("ParkingAllocation", allocation.Id.ToString(), "Release", null, $"Released parking lot {allocation.ParkingLot?.LotNumber}");

        return OkResponse("Parking allocation released successfully.");
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<List<ParkingAllocationHistoryDto>>>> GetAllocationHistory([FromQuery] int? lotId)
    {
        var query = _context.ParkingAllocations
            .Include(a => a.ParkingLot)
            .Include(a => a.Flat)
            .Include(a => a.Resident)
            .AsNoTracking()
            .AsQueryable();

        if (lotId.HasValue)
        {
            query = query.Where(a => a.ParkingLotId == lotId.Value);
        }

        var history = await query
            .OrderByDescending(a => a.StartDateUtc)
            .Take(50)
            .Select(a => new ParkingAllocationHistoryDto
            {
                Id = a.Id,
                LotNumber = a.ParkingLot.LotNumber,
                FlatNumber = a.Flat.FlatNumber,
                ResidentName = a.Resident != null ? a.Resident.FullName : "N/A",
                VehicleNumber = a.VehicleNumber,
                StartDateUtc = a.StartDateUtc,
                EndDateUtc = a.EndDateUtc,
                IsActive = a.IsActive
            })
            .ToListAsync();

        return OkResponse(history);
    }
}
