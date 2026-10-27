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
public class AssetsController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public AssetsController(IApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<List<AssetCategory>>>> GetCategories()
    {
        var categories = await _context.AssetCategories.OrderBy(c => c.Name).ToListAsync();
        return OkResponse(categories);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AssetDto>>>> GetAssets(
        [FromQuery] int? categoryId,
        [FromQuery] int? towerId,
        [FromQuery] AssetCondition? condition,
        [FromQuery] bool? dueForMaintenanceOnly,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Assets
            .Include(a => a.Category)
            .Include(a => a.Tower)
            .Include(a => a.AssignedVendor)
            .AsNoTracking()
            .AsQueryable();

        if (categoryId.HasValue) query = query.Where(a => a.CategoryId == categoryId.Value);
        if (towerId.HasValue) query = query.Where(a => a.TowerId == towerId.Value);
        if (condition.HasValue) query = query.Where(a => a.Condition == condition.Value);

        if (dueForMaintenanceOnly == true)
        {
            var today = DateTime.UtcNow.Date;
            query = query.Where(a => a.NextScheduledMaintenanceDate <= today.AddDays(7) || a.Condition == AssetCondition.NeedsMaintenance);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(a => a.AssetCode.ToLower().Contains(searchLower) ||
                                     a.AssetName.ToLower().Contains(searchLower) ||
                                     (a.Manufacturer != null && a.Manufacturer.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync();
        var assets = await query
            .OrderBy(a => a.AssetCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AssetDto
            {
                Id = a.Id,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryId = a.CategoryId,
                CategoryName = a.Category.Name,
                Description = a.Description,
                Manufacturer = a.Manufacturer,
                ModelNumber = a.ModelNumber,
                SerialNumber = a.SerialNumber,
                PurchaseDate = a.PurchaseDate,
                PurchaseCost = a.PurchaseCost,
                WarrantyExpiryDate = a.WarrantyExpiryDate,
                TowerId = a.TowerId,
                TowerName = a.Tower != null ? a.Tower.TowerName : null,
                SpecificLocation = a.SpecificLocation,
                AssignedVendorId = a.AssignedVendorId,
                AssignedVendorName = a.AssignedVendor != null ? a.AssignedVendor.VendorName : null,
                Condition = a.Condition.ToString(),
                Status = a.Status,
                LastMaintenanceDate = a.LastMaintenanceDate,
                NextScheduledMaintenanceDate = a.NextScheduledMaintenanceDate,
                IsActive = a.IsActive
            })
            .ToListAsync();

        return OkResponse(new PagedResult<AssetDto>(assets, totalCount, page, pageSize));
    }

    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary},{SystemRoles.MaintenanceStaff}")]
    public async Task<ActionResult<ApiResponse<AssetDto>>> CreateAsset([FromBody] CreateAssetDto request)
    {
        if (await _context.Assets.AnyAsync(a => a.AssetCode == request.AssetCode.Trim()))
        {
            return FailResponse<AssetDto>("An asset with this Asset Code already exists.");
        }

        var asset = new Asset
        {
            AssetCode = request.AssetCode.Trim(),
            AssetName = request.AssetName.Trim(),
            CategoryId = request.CategoryId,
            Description = request.Description?.Trim(),
            Manufacturer = request.Manufacturer?.Trim(),
            ModelNumber = request.ModelNumber?.Trim(),
            SerialNumber = request.SerialNumber?.Trim(),
            PurchaseDate = request.PurchaseDate,
            PurchaseCost = request.PurchaseCost,
            WarrantyExpiryDate = request.WarrantyExpiryDate,
            TowerId = request.TowerId,
            SpecificLocation = request.SpecificLocation?.Trim(),
            AssignedVendorId = request.AssignedVendorId,
            Condition = request.Condition,
            NextScheduledMaintenanceDate = request.NextScheduledMaintenanceDate,
            Status = "Operational",
            IsActive = true
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("Asset", asset.Id.ToString(), "Create", null, $"Created asset {asset.AssetCode} ({asset.AssetName})");

        return OkResponse(new AssetDto
        {
            Id = asset.Id,
            AssetCode = asset.AssetCode,
            AssetName = asset.AssetName,
            CategoryId = asset.CategoryId,
            Condition = asset.Condition.ToString(),
            Status = asset.Status
        }, "Asset registered successfully.");
    }

    [HttpGet("{id}/maintenance")]
    public async Task<ActionResult<ApiResponse<List<AssetMaintenanceRecordDto>>>> GetMaintenanceRecords(int id)
    {
        var records = await _context.AssetMaintenanceRecords
            .Include(r => r.Asset)
            .Include(r => r.Vendor)
            .Where(r => r.AssetId == id)
            .OrderByDescending(r => r.MaintenanceDate)
            .Select(r => new AssetMaintenanceRecordDto
            {
                Id = r.Id,
                AssetId = r.AssetId,
                AssetName = r.Asset.AssetName,
                MaintenanceDate = r.MaintenanceDate,
                MaintenanceType = r.MaintenanceType.ToString(),
                PerformedBy = r.PerformedBy,
                VendorId = r.VendorId,
                VendorName = r.Vendor != null ? r.Vendor.VendorName : null,
                CostIncurred = r.CostIncurred,
                FindingsAndWorkDone = r.FindingsAndWorkDone,
                PartsReplaced = r.PartsReplaced,
                NextRecommendedServiceDate = r.NextRecommendedServiceDate
            })
            .ToListAsync();

        return OkResponse(records);
    }

    [HttpPost("{id}/maintenance")]
    [Authorize(Roles = $"{SystemRoles.SystemAdmin},{SystemRoles.Secretary},{SystemRoles.MaintenanceStaff}")]
    public async Task<ActionResult<ApiResponse<string>>> AddMaintenanceRecord(int id, [FromBody] CreateAssetMaintenanceRecordDto request)
    {
        var asset = await _context.Assets.FirstOrDefaultAsync(a => a.Id == id);
        if (asset == null) return FailResponse<string>("Asset not found.", statusCode: 404);

        var record = new AssetMaintenanceRecord
        {
            AssetId = id,
            MaintenanceDate = request.MaintenanceDate,
            MaintenanceType = request.MaintenanceType,
            PerformedBy = request.PerformedBy.Trim(),
            VendorId = request.VendorId,
            CostIncurred = request.CostIncurred,
            FindingsAndWorkDone = request.FindingsAndWorkDone.Trim(),
            PartsReplaced = request.PartsReplaced?.Trim(),
            NextRecommendedServiceDate = request.NextRecommendedServiceDate,
            IsActive = true
        };

        asset.LastMaintenanceDate = request.MaintenanceDate;
        if (request.NextRecommendedServiceDate.HasValue)
        {
            asset.NextScheduledMaintenanceDate = request.NextRecommendedServiceDate.Value;
        }

        asset.Condition = AssetCondition.Good;

        _context.AssetMaintenanceRecords.Add(record);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("AssetMaintenanceRecord", record.Id.ToString(), "Create", null, $"Logged maintenance for asset {asset.AssetCode}");

        return OkResponse("Maintenance activity recorded successfully.");
    }
}
