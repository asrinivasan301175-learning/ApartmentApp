namespace ApartmentMaintenance.Application.DTOs;

public class DashboardMetricsDto
{
    public int TotalTowers { get; set; }
    public int TotalFlats { get; set; }
    public int OccupiedFlats { get; set; }
    public int VacantFlats { get; set; }
    public double OccupancyRatePercentage => TotalFlats > 0 ? Math.Round((double)OccupiedFlats / TotalFlats * 100, 1) : 0;

    public int TotalParkingLots { get; set; }
    public int AllocatedParkingLots { get; set; }
    public int AvailableParkingLots { get; set; }

    public int TotalResidents { get; set; }
    public int TotalEmployees { get; set; }
    public int ActiveVendors { get; set; }
    public int ContractsExpiringIn30Days { get; set; }

    public int TotalAssets { get; set; }
    public int AssetsDueForMaintenance { get; set; }

    public int PendingPurchaseRequests { get; set; }
    public int MyPendingApprovalsCount { get; set; }
    public int ApprovedPurchaseOrdersThisMonth { get; set; }
    public decimal TotalApprovedSpendThisMonth { get; set; }

    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class RecentActivityDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class CommitteeMemberDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public string MemberPhone { get; set; } = string.Empty;
    public string CommitteeRole { get; set; } = string.Empty;
    public DateTime TermStartDateUtc { get; set; }
    public DateTime TermEndDateUtc { get; set; }
    public bool IsCurrentActive { get; set; }
    public string? Remarks { get; set; }
}

public class AssignCommitteeMemberDto
{
    public Guid UserId { get; set; }
    public string CommitteeRole { get; set; } = string.Empty; // President, Secretary, Treasurer
    public DateTime TermStartDateUtc { get; set; }
    public DateTime TermEndDateUtc { get; set; }
    public string? Remarks { get; set; }
}
