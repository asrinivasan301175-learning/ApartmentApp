using Microsoft.EntityFrameworkCore;
using ApartmentMaintenance.Application.Common;
using ApartmentMaintenance.Application.Interfaces;
using ApartmentMaintenance.Domain.Common;
using ApartmentMaintenance.Domain.Entities;

namespace ApartmentMaintenance.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRoleMapping> UserRoles => Set<UserRoleMapping>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Tower> Towers => Set<Tower>();
    public DbSet<Flat> Flats => Set<Flat>();
    public DbSet<Resident> Residents => Set<Resident>();
    public DbSet<FlatOwnership> FlatOwnerships => Set<FlatOwnership>();
    public DbSet<FlatOccupancy> FlatOccupancies => Set<FlatOccupancy>();

    public DbSet<ParkingLot> ParkingLots => Set<ParkingLot>();
    public DbSet<ParkingAllocation> ParkingAllocations => Set<ParkingAllocation>();

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<VendorCategory> VendorCategories => Set<VendorCategory>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<VendorContract> VendorContracts => Set<VendorContract>();

    public DbSet<AssetCategory> AssetCategories => Set<AssetCategory>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetMaintenanceRecord> AssetMaintenanceRecords => Set<AssetMaintenanceRecord>();

    public DbSet<CommitteeMember> CommitteeMembers => Set<CommitteeMember>();
    public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
    public DbSet<PurchaseRequestItem> PurchaseRequestItems => Set<PurchaseRequestItem>();
    public DbSet<PurchaseRequestAttachment> PurchaseRequestAttachments => Set<PurchaseRequestAttachment>();
    public DbSet<PurchaseApproval> PurchaseApprovals => Set<PurchaseApproval>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply Global Query Filters for Soft Deletes
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Tower>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Flat>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Resident>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ParkingLot>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Vendor>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Asset>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PurchaseRequest>().HasQueryFilter(e => !e.IsDeleted);

        // User & Auth
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.NormalizedEmail).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.NormalizedEmail).HasMaxLength(256).IsRequired();
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.PhoneNumber).HasMaxLength(30);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(r => r.NormalizedName).IsUnique();
            entity.Property(r => r.Name).HasMaxLength(100).IsRequired();
            entity.Property(r => r.NormalizedName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<UserRoleMapping>(entity =>
        {
            entity.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
            entity.HasOne(ur => ur.User)
                  .WithMany(u => u.UserRoles)
                  .HasForeignKey(ur => ur.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                  .WithMany(r => r.UserRoles)
                  .HasForeignKey(ur => ur.RoleId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Towers and Flats
        modelBuilder.Entity<Tower>(entity =>
        {
            entity.HasIndex(t => t.TowerCode).IsUnique();
            entity.Property(t => t.TowerCode).HasMaxLength(20).IsRequired();
            entity.Property(t => t.TowerName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Flat>(entity =>
        {
            entity.HasIndex(f => new { f.TowerId, f.FlatNumber }).IsUnique();
            entity.Property(f => f.FlatNumber).HasMaxLength(20).IsRequired();
            entity.Property(f => f.AreaSqFt).HasPrecision(10, 2);
            entity.HasOne(f => f.Tower)
                  .WithMany(t => t.Flats)
                  .HasForeignKey(f => f.TowerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Residents and Ownership/Occupancy
        modelBuilder.Entity<Resident>(entity =>
        {
            entity.Property(r => r.FullName).HasMaxLength(150).IsRequired();
            entity.Property(r => r.Email).HasMaxLength(256);
            entity.Property(r => r.MobileNumber).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<FlatOwnership>(entity =>
        {
            entity.Property(fo => fo.OwnershipPercentage).HasPrecision(5, 2);
            entity.HasOne(fo => fo.Flat)
                  .WithMany(f => f.Ownerships)
                  .HasForeignKey(fo => fo.FlatId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(fo => fo.Resident)
                  .WithMany(r => r.FlatOwnerships)
                  .HasForeignKey(fo => fo.ResidentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FlatOccupancy>(entity =>
        {
            entity.HasOne(fo => fo.Flat)
                  .WithMany(f => f.Occupancies)
                  .HasForeignKey(fo => fo.FlatId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(fo => fo.Resident)
                  .WithMany(r => r.FlatOccupancies)
                  .HasForeignKey(fo => fo.ResidentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Parking
        modelBuilder.Entity<ParkingLot>(entity =>
        {
            entity.HasIndex(p => p.LotNumber).IsUnique();
            entity.Property(p => p.LotNumber).HasMaxLength(30).IsRequired();
            entity.HasOne(p => p.Tower)
                  .WithMany(t => t.ParkingLots)
                  .HasForeignKey(p => p.TowerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ParkingAllocation>(entity =>
        {
            entity.HasOne(pa => pa.ParkingLot)
                  .WithMany(p => p.Allocations)
                  .HasForeignKey(pa => pa.ParkingLotId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(pa => pa.Flat)
                  .WithMany(f => f.ParkingAllocations)
                  .HasForeignKey(pa => pa.FlatId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(pa => pa.Resident)
                  .WithMany(r => r.ParkingAllocations)
                  .HasForeignKey(pa => pa.ResidentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Employee
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.Property(e => e.EmployeeCode).HasMaxLength(30).IsRequired();
            entity.Property(e => e.MonthlySalary).HasPrecision(18, 2);
        });

        // Vendors and Contracts
        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.Property(v => v.VendorName).HasMaxLength(150).IsRequired();
            entity.HasOne(v => v.Category)
                  .WithMany(c => c.Vendors)
                  .HasForeignKey(v => v.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<VendorContract>(entity =>
        {
            entity.Property(vc => vc.ContractValue).HasPrecision(18, 2);
            entity.HasOne(vc => vc.Vendor)
                  .WithMany(v => v.Contracts)
                  .HasForeignKey(vc => vc.VendorId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Assets and Maintenance
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasIndex(a => a.AssetCode).IsUnique();
            entity.Property(a => a.AssetCode).HasMaxLength(50).IsRequired();
            entity.Property(a => a.AssetName).HasMaxLength(150).IsRequired();
            entity.Property(a => a.PurchaseCost).HasPrecision(18, 2);
            entity.HasOne(a => a.Category)
                  .WithMany(c => c.Assets)
                  .HasForeignKey(a => a.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(a => a.Tower)
                  .WithMany(t => t.Assets)
                  .HasForeignKey(a => a.TowerId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(a => a.AssignedVendor)
                  .WithMany(v => v.Assets)
                  .HasForeignKey(a => a.AssignedVendorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AssetMaintenanceRecord>(entity =>
        {
            entity.Property(mr => mr.CostIncurred).HasPrecision(18, 2);
            entity.HasOne(mr => mr.Asset)
                  .WithMany(a => a.MaintenanceRecords)
                  .HasForeignKey(mr => mr.AssetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Purchase Workflow
        modelBuilder.Entity<PurchaseRequest>(entity =>
        {
            entity.HasIndex(pr => pr.RequestNumber).IsUnique();
            entity.Property(pr => pr.RequestNumber).HasMaxLength(50).IsRequired();
            entity.Property(pr => pr.Title).HasMaxLength(150).IsRequired();
            entity.Property(pr => pr.EstimatedTotalAmount).HasPrecision(18, 2);
            entity.Property(pr => pr.TaxAndOtherCharges).HasPrecision(18, 2);

            entity.HasOne(pr => pr.Requester)
                  .WithMany()
                  .HasForeignKey(pr => pr.RequesterId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pr => pr.Vendor)
                  .WithMany(v => v.PurchaseRequests)
                  .HasForeignKey(pr => pr.VendorId)
                  .OnDelete(DeleteBehavior.SetNull);

            // RowVersion for optimistic concurrency
            entity.Property(pr => pr.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<PurchaseRequestItem>(entity =>
        {
            entity.Property(i => i.EstimatedUnitPrice).HasPrecision(18, 2);
            entity.HasOne(i => i.PurchaseRequest)
                  .WithMany(pr => pr.Items)
                  .HasForeignKey(i => i.PurchaseRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PurchaseApproval>(entity =>
        {
            entity.HasOne(pa => pa.PurchaseRequest)
                  .WithMany(pr => pr.Approvals)
                  .HasForeignKey(pa => pa.PurchaseRequestId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pa => pa.Approver)
                  .WithMany()
                  .HasForeignKey(pa => pa.ApproverId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.HasIndex(po => po.PoNumber).IsUnique();
            entity.Property(po => po.PoNumber).HasMaxLength(50).IsRequired();
            entity.Property(po => po.TotalAmount).HasPrecision(18, 2);
            entity.Property(po => po.TaxAmount).HasPrecision(18, 2);

            entity.HasOne(po => po.PurchaseRequest)
                  .WithOne(pr => pr.PurchaseOrder)
                  .HasForeignKey<PurchaseOrder>(po => po.PurchaseRequestId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(po => po.Vendor)
                  .WithMany(v => v.PurchaseOrders)
                  .HasForeignKey(po => po.VendorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PurchaseOrderItem>(entity =>
        {
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(i => i.PurchaseOrder)
                  .WithMany(po => po.Items)
                  .HasForeignKey(i => i.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService?.UserId?.ToString() ?? "System";
        var currentTime = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditableEntity<int> intAuditable)
            {
                if (entry.State == EntityState.Added)
                {
                    intAuditable.CreatedAtUtc = currentTime;
                    intAuditable.CreatedBy = currentUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    intAuditable.UpdatedAtUtc = currentTime;
                    intAuditable.UpdatedBy = currentUserId;
                }
            }
            else if (entry.Entity is AuditableEntity<Guid> guidAuditable)
            {
                if (entry.State == EntityState.Added)
                {
                    guidAuditable.CreatedAtUtc = currentTime;
                    guidAuditable.CreatedBy = currentUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    guidAuditable.UpdatedAtUtc = currentTime;
                    guidAuditable.UpdatedBy = currentUserId;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
