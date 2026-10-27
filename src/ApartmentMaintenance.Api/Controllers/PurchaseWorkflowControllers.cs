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
public class PurchaseRequestsController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<PurchaseRequestsController> _logger;

    public PurchaseRequestsController(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService,
        ILogger<PurchaseRequestsController> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PurchaseRequestDto>>>> GetPurchaseRequests(
        [FromQuery] PurchaseRequestStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.PurchaseRequests
            .Include(pr => pr.Requester)
            .Include(pr => pr.Vendor)
            .Include(pr => pr.PurchaseOrder)
            .AsNoTracking()
            .AsQueryable();

        // If user is resident or ordinary staff, they only see their own requests unless they have committee roles
        var isCommitteeOrAdmin = _currentUserService.IsInRole(SystemRoles.SystemAdmin) ||
                                 _currentUserService.IsInRole(SystemRoles.President) ||
                                 _currentUserService.IsInRole(SystemRoles.Secretary) ||
                                 _currentUserService.IsInRole(SystemRoles.Treasurer);

        if (!isCommitteeOrAdmin && _currentUserService.UserId.HasValue)
        {
            query = query.Where(pr => pr.RequesterId == _currentUserService.UserId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(pr => pr.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(pr => pr.RequestNumber.ToLower().Contains(searchLower) ||
                                      pr.Title.ToLower().Contains(searchLower) ||
                                      (pr.Vendor != null && pr.Vendor.VendorName.ToLower().Contains(searchLower)));
        }

        var totalCount = await query.CountAsync();
        var requests = await query
            .OrderByDescending(pr => pr.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pr => new PurchaseRequestDto
            {
                Id = pr.Id,
                RequestNumber = pr.RequestNumber,
                Title = pr.Title,
                Description = pr.Description,
                RequesterId = pr.RequesterId,
                RequesterName = pr.Requester.FullName,
                VendorId = pr.VendorId,
                VendorName = pr.Vendor != null ? pr.Vendor.VendorName : null,
                EstimatedTotalAmount = pr.EstimatedTotalAmount,
                TaxAndOtherCharges = pr.TaxAndOtherCharges,
                GrandTotal = pr.GrandTotal,
                BudgetCategory = pr.BudgetCategory,
                RequiredDeliveryDate = pr.RequiredDeliveryDate,
                Status = pr.Status.ToString(),
                CurrentApprovalStep = pr.CurrentApprovalStep,
                CurrentAssignedRole = pr.CurrentAssignedRole,
                CreatedAtUtc = pr.CreatedAtUtc,
                RowVersion = pr.RowVersion,
                PurchaseOrderId = pr.PurchaseOrder != null ? pr.PurchaseOrder.Id : null,
                PurchaseOrderNumber = pr.PurchaseOrder != null ? pr.PurchaseOrder.PoNumber : null
            })
            .ToListAsync();

        return OkResponse(new PagedResult<PurchaseRequestDto>(requests, totalCount, page, pageSize));
    }

    [HttpGet("my-pending-approvals")]
    public async Task<ActionResult<ApiResponse<List<PurchaseRequestDto>>>> GetMyPendingApprovals()
    {
        var userRoles = _currentUserService.Roles;
        var currentUserId = _currentUserService.UserId;

        var query = _context.PurchaseRequests
            .Include(pr => pr.Requester)
            .Include(pr => pr.Vendor)
            .Include(pr => pr.Items)
            .Where(pr => pr.Status == PurchaseRequestStatus.PendingApproval)
            .AsNoTracking()
            .AsQueryable();

        // Determine which requests are currently pending for user's committee role
        query = query.Where(pr =>
            (pr.CurrentApprovalStep == 1 && userRoles.Contains(SystemRoles.President)) ||
            (pr.CurrentApprovalStep == 2 && userRoles.Contains(SystemRoles.Secretary)) ||
            (pr.CurrentApprovalStep == 3 && userRoles.Contains(SystemRoles.Treasurer)) ||
            userRoles.Contains(SystemRoles.SystemAdmin));

        // Requesters should not approve their own requests
        if (currentUserId.HasValue && !userRoles.Contains(SystemRoles.SystemAdmin))
        {
            query = query.Where(pr => pr.RequesterId != currentUserId.Value);
        }

        var list = await query
            .OrderBy(pr => pr.RequiredDeliveryDate)
            .Select(pr => new PurchaseRequestDto
            {
                Id = pr.Id,
                RequestNumber = pr.RequestNumber,
                Title = pr.Title,
                Description = pr.Description,
                RequesterId = pr.RequesterId,
                RequesterName = pr.Requester.FullName,
                VendorId = pr.VendorId,
                VendorName = pr.Vendor != null ? pr.Vendor.VendorName : null,
                EstimatedTotalAmount = pr.EstimatedTotalAmount,
                TaxAndOtherCharges = pr.TaxAndOtherCharges,
                GrandTotal = pr.GrandTotal,
                BudgetCategory = pr.BudgetCategory,
                RequiredDeliveryDate = pr.RequiredDeliveryDate,
                Status = pr.Status.ToString(),
                CurrentApprovalStep = pr.CurrentApprovalStep,
                CurrentAssignedRole = pr.CurrentAssignedRole,
                CreatedAtUtc = pr.CreatedAtUtc,
                RowVersion = pr.RowVersion
            })
            .ToListAsync();

        return OkResponse(list);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PurchaseRequestDto>>> GetById(Guid id)
    {
        var pr = await _context.PurchaseRequests
            .Include(p => p.Requester)
            .Include(p => p.Vendor)
            .Include(p => p.Items).ThenInclude(i => i.RelatedAsset)
            .Include(p => p.Approvals).ThenInclude(a => a.Approver)
            .Include(p => p.PurchaseOrder)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pr == null) return FailResponse<PurchaseRequestDto>("Purchase request not found.", statusCode: 404);

        var dto = new PurchaseRequestDto
        {
            Id = pr.Id,
            RequestNumber = pr.RequestNumber,
            Title = pr.Title,
            Description = pr.Description,
            RequesterId = pr.RequesterId,
            RequesterName = pr.Requester.FullName,
            VendorId = pr.VendorId,
            VendorName = pr.Vendor?.VendorName,
            EstimatedTotalAmount = pr.EstimatedTotalAmount,
            TaxAndOtherCharges = pr.TaxAndOtherCharges,
            GrandTotal = pr.GrandTotal,
            BudgetCategory = pr.BudgetCategory,
            RequiredDeliveryDate = pr.RequiredDeliveryDate,
            Status = pr.Status.ToString(),
            CurrentApprovalStep = pr.CurrentApprovalStep,
            CurrentAssignedRole = pr.CurrentAssignedRole,
            CreatedAtUtc = pr.CreatedAtUtc,
            RowVersion = pr.RowVersion,
            PurchaseOrderId = pr.PurchaseOrder?.Id,
            PurchaseOrderNumber = pr.PurchaseOrder?.PoNumber,
            Items = pr.Items.Select(i => new PurchaseRequestItemDto
            {
                Id = i.Id,
                ItemDescription = i.ItemDescription,
                Quantity = i.Quantity,
                UnitOfMeasure = i.UnitOfMeasure,
                EstimatedUnitPrice = i.EstimatedUnitPrice,
                TotalEstimatedPrice = i.TotalEstimatedPrice,
                RelatedAssetId = i.RelatedAssetId,
                RelatedAssetName = i.RelatedAsset?.AssetName
            }).ToList(),
            Approvals = pr.Approvals.OrderBy(a => a.StepOrder).Select(a => new PurchaseApprovalDto
            {
                Id = a.Id,
                ApproverId = a.ApproverId,
                ApproverName = a.Approver.FullName,
                ApproverRole = a.ApproverRole,
                StepOrder = a.StepOrder,
                Decision = a.Decision.ToString(),
                Comments = a.Comments,
                DecisionDateUtc = a.DecisionDateUtc
            }).ToList()
        };

        return OkResponse(dto);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PurchaseRequestDto>>> CreateRequest([FromBody] CreatePurchaseRequestDto request)
    {
        var requesterId = _currentUserService.UserId;
        if (!requesterId.HasValue) return FailResponse<PurchaseRequestDto>("Unauthorized", statusCode: 401);

        var count = await _context.PurchaseRequests.CountAsync() + 1;
        var requestNumber = $"PR-{DateTime.UtcNow.Year}-{count:D4}";

        var totalItemCost = request.Items.Sum(i => i.Quantity * i.EstimatedUnitPrice);

        var pr = new PurchaseRequest
        {
            RequestNumber = requestNumber,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            RequesterId = requesterId.Value,
            VendorId = request.VendorId,
            BudgetCategory = request.BudgetCategory.Trim(),
            RequiredDeliveryDate = request.RequiredDeliveryDate,
            EstimatedTotalAmount = totalItemCost,
            TaxAndOtherCharges = request.TaxAndOtherCharges,
            Status = request.SubmitImmediately ? PurchaseRequestStatus.PendingApproval : PurchaseRequestStatus.Draft,
            CurrentApprovalStep = 1,
            CurrentAssignedRole = SystemRoles.President,
            IsActive = true
        };

        foreach (var item in request.Items)
        {
            pr.Items.Add(new PurchaseRequestItem
            {
                ItemDescription = item.ItemDescription.Trim(),
                Quantity = item.Quantity,
                UnitOfMeasure = item.UnitOfMeasure.Trim(),
                EstimatedUnitPrice = item.EstimatedUnitPrice,
                RelatedAssetId = item.RelatedAssetId
            });
        }

        _context.PurchaseRequests.Add(pr);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("PurchaseRequest", pr.Id.ToString(), "Create", null, $"Created PR {pr.RequestNumber}: {pr.Title}");

        return OkResponse(new PurchaseRequestDto
        {
            Id = pr.Id,
            RequestNumber = pr.RequestNumber,
            Title = pr.Title,
            Status = pr.Status.ToString(),
            GrandTotal = pr.GrandTotal
        }, "Purchase request created successfully.");
    }

    [HttpPost("{id}/approval-action")]
    [Authorize(Roles = $"{SystemRoles.President},{SystemRoles.Secretary},{SystemRoles.Treasurer},{SystemRoles.SystemAdmin}")]
    public async Task<ActionResult<ApiResponse<string>>> ProcessApprovalAction(Guid id, [FromBody] PurchaseApprovalActionDto request)
    {
        var approverId = _currentUserService.UserId;
        if (!approverId.HasValue) return FailResponse<string>("Unauthorized", statusCode: 401);

        var pr = await _context.PurchaseRequests
            .Include(p => p.Approvals)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pr == null) return FailResponse<string>("Purchase request not found.", statusCode: 404);

        if (pr.Status != PurchaseRequestStatus.PendingApproval)
        {
            return FailResponse<string>($"Request is not in PendingApproval status (Current: {pr.Status}).");
        }

        // Verify that requester is not approving their own request unless Admin
        if (pr.RequesterId == approverId.Value && !_currentUserService.IsInRole(SystemRoles.SystemAdmin))
        {
            return FailResponse<string>("Requester cannot approve their own purchase request.");
        }

        // Sequential validation: Verify current step role matches user's active role
        var currentRoleNeeded = pr.CurrentApprovalStep switch
        {
            1 => SystemRoles.President,
            2 => SystemRoles.Secretary,
            3 => SystemRoles.Treasurer,
            _ => null
        };

        if (currentRoleNeeded != null && !_currentUserService.IsInRole(currentRoleNeeded) && !_currentUserService.IsInRole(SystemRoles.SystemAdmin))
        {
            return FailResponse<string>($"This request is currently awaiting approval from the {currentRoleNeeded}.");
        }

        // Optimistic concurrency check if RowVersion provided
        if (request.RowVersion != null && pr.RowVersion != null && !request.RowVersion.SequenceEqual(pr.RowVersion))
        {
            return FailResponse<string>("This request was updated by another committee member simultaneously. Please refresh.", statusCode: 409);
        }

        var approvalRecord = new PurchaseApproval
        {
            PurchaseRequestId = pr.Id,
            ApproverId = approverId.Value,
            ApproverRole = currentRoleNeeded ?? "Administrator",
            StepOrder = pr.CurrentApprovalStep,
            Decision = request.Decision,
            Comments = request.Comments?.Trim() ?? string.Empty,
            DecisionDateUtc = DateTime.UtcNow
        };

        _context.PurchaseApprovals.Add(approvalRecord);

        if (request.Decision == ApprovalDecision.Rejected)
        {
            pr.Status = PurchaseRequestStatus.Rejected;
            pr.CurrentAssignedRole = null;
        }
        else if (request.Decision == ApprovalDecision.ReturnedForChanges)
        {
            pr.Status = PurchaseRequestStatus.ReturnedForChanges;
            pr.CurrentApprovalStep = 1;
            pr.CurrentAssignedRole = null;
        }
        else if (request.Decision == ApprovalDecision.Approved)
        {
            if (pr.CurrentApprovalStep == 1) // President approved -> Send to Secretary
            {
                pr.CurrentApprovalStep = 2;
                pr.CurrentAssignedRole = SystemRoles.Secretary;
            }
            else if (pr.CurrentApprovalStep == 2) // Secretary approved -> Send to Treasurer
            {
                pr.CurrentApprovalStep = 3;
                pr.CurrentAssignedRole = SystemRoles.Treasurer;
            }
            else if (pr.CurrentApprovalStep == 3) // Treasurer approved -> Complete
            {
                pr.Status = PurchaseRequestStatus.Approved;
                pr.CurrentApprovalStep = 4;
                pr.CurrentAssignedRole = null;
            }
        }

        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("PurchaseRequest", pr.Id.ToString(), $"Approval-{request.Decision}", null, 
            $"{approvalRecord.ApproverRole} recorded decision {request.Decision}. Comments: {request.Comments}");

        return OkResponse($"Purchase request decision recorded as {request.Decision}. Current state: {pr.Status}.");
    }
}

[Authorize]
public class PurchaseOrdersController : BaseApiController
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public PurchaseOrdersController(IApplicationDbContext context, IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PurchaseOrderDto>>>> GetPurchaseOrders()
    {
        var orders = await _context.PurchaseOrders
            .Include(po => po.PurchaseRequest)
            .Include(po => po.Vendor)
            .Include(po => po.Items)
            .OrderByDescending(po => po.OrderDateUtc)
            .Select(po => new PurchaseOrderDto
            {
                Id = po.Id,
                PoNumber = po.PoNumber,
                PurchaseRequestId = po.PurchaseRequestId,
                PurchaseRequestNumber = po.PurchaseRequest.RequestNumber,
                VendorId = po.VendorId,
                VendorName = po.Vendor.VendorName,
                OrderDateUtc = po.OrderDateUtc,
                ExpectedDeliveryDate = po.ExpectedDeliveryDate,
                TotalAmount = po.TotalAmount,
                TaxAmount = po.TaxAmount,
                GrandTotal = po.GrandTotal,
                DeliveryStatus = po.DeliveryStatus.ToString(),
                TermsAndConditions = po.TermsAndConditions,
                Items = po.Items.Select(i => new PurchaseOrderItemDto
                {
                    Id = i.Id,
                    ItemDescription = i.ItemDescription,
                    Quantity = i.Quantity,
                    UnitOfMeasure = i.UnitOfMeasure,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            })
            .ToListAsync();

        return OkResponse(orders);
    }

    [HttpPost("generate/{purchaseRequestId}")]
    [Authorize(Roles = $"{SystemRoles.President},{SystemRoles.Secretary},{SystemRoles.Treasurer},{SystemRoles.SystemAdmin}")]
    public async Task<ActionResult<ApiResponse<PurchaseOrderDto>>> GeneratePurchaseOrder(Guid purchaseRequestId)
    {
        var pr = await _context.PurchaseRequests
            .Include(p => p.Items)
            .Include(p => p.Vendor)
            .Include(p => p.PurchaseOrder)
            .FirstOrDefaultAsync(p => p.Id == purchaseRequestId);

        if (pr == null) return FailResponse<PurchaseOrderDto>("Purchase request not found.", statusCode: 404);

        if (pr.Status != PurchaseRequestStatus.Approved)
        {
            return FailResponse<PurchaseOrderDto>("Cannot generate Purchase Order: Purchase request is not Approved yet.");
        }

        if (pr.PurchaseOrder != null)
        {
            return FailResponse<PurchaseOrderDto>("A Purchase Order has already been generated for this request.");
        }

        if (!pr.VendorId.HasValue)
        {
            return FailResponse<PurchaseOrderDto>("A vendor must be assigned to generate a Purchase Order.");
        }

        var count = await _context.PurchaseOrders.CountAsync() + 1;
        var poNumber = $"PO-{DateTime.UtcNow.Year}-{count:D4}";

        var po = new PurchaseOrder
        {
            PoNumber = poNumber,
            PurchaseRequestId = pr.Id,
            VendorId = pr.VendorId.Value,
            OrderDateUtc = DateTime.UtcNow,
            ExpectedDeliveryDate = pr.RequiredDeliveryDate,
            TotalAmount = pr.EstimatedTotalAmount,
            TaxAmount = pr.TaxAndOtherCharges,
            DeliveryStatus = PurchaseOrderStatus.Issued,
            TermsAndConditions = "Payment upon verified delivery and technician sign-off."
        };

        foreach (var item in pr.Items)
        {
            po.Items.Add(new PurchaseOrderItem
            {
                ItemDescription = item.ItemDescription,
                Quantity = item.Quantity,
                UnitOfMeasure = item.UnitOfMeasure,
                UnitPrice = item.EstimatedUnitPrice
            });
        }

        _context.PurchaseOrders.Add(po);
        await _context.SaveChangesAsync();
        await _auditLogService.LogAsync("PurchaseOrder", po.Id.ToString(), "GeneratePO", null, $"Generated {po.PoNumber} from {pr.RequestNumber}");

        return OkResponse(new PurchaseOrderDto
        {
            Id = po.Id,
            PoNumber = po.PoNumber,
            PurchaseRequestId = pr.Id,
            PurchaseRequestNumber = pr.RequestNumber,
            VendorId = po.VendorId,
            TotalAmount = po.TotalAmount,
            TaxAmount = po.TaxAmount,
            GrandTotal = po.GrandTotal,
            DeliveryStatus = po.DeliveryStatus.ToString()
        }, "Purchase Order successfully generated.");
    }
}
