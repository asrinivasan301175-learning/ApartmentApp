using Microsoft.EntityFrameworkCore;
using ApartmentMaintenance.Domain.Entities;

namespace ApartmentMaintenance.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRoleMapping> UserRoles { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Notification> Notifications { get; }

    DbSet<Tower> Towers { get; }
    DbSet<Flat> Flats { get; }
    DbSet<Resident> Residents { get; }
    DbSet<FlatOwnership> FlatOwnerships { get; }
    DbSet<FlatOccupancy> FlatOccupancies { get; }

    DbSet<ParkingLot> ParkingLots { get; }
    DbSet<ParkingAllocation> ParkingAllocations { get; }

    DbSet<Employee> Employees { get; }
    DbSet<VendorCategory> VendorCategories { get; }
    DbSet<Vendor> Vendors { get; }
    DbSet<VendorContract> VendorContracts { get; }

    DbSet<AssetCategory> AssetCategories { get; }
    DbSet<Asset> Assets { get; }
    DbSet<AssetMaintenanceRecord> AssetMaintenanceRecords { get; }

    DbSet<CommitteeMember> CommitteeMembers { get; }
    DbSet<PurchaseRequest> PurchaseRequests { get; }
    DbSet<PurchaseRequestItem> PurchaseRequestItems { get; }
    DbSet<PurchaseRequestAttachment> PurchaseRequestAttachments { get; }
    DbSet<PurchaseApproval> PurchaseApprovals { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderItem> PurchaseOrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
