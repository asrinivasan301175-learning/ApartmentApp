using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ApartmentMaintenance.Application.Interfaces;
using ApartmentMaintenance.Domain.Entities;
using ApartmentMaintenance.Domain.Enums;
using ApartmentMaintenance.Infrastructure.Persistence;

namespace ApartmentMaintenance.Infrastructure.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ILogger logger)
    {
        try
        {
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            if (!await context.Roles.AnyAsync())
            {
                logger.LogInformation("Seeding standard system roles...");
                var roles = new List<Role>
                {
                    new() { Name = SystemRoles.SystemAdmin, NormalizedName = SystemRoles.SystemAdmin.ToUpperInvariant(), Description = "Full system administration and configuration" },
                    new() { Name = SystemRoles.President, NormalizedName = SystemRoles.President.ToUpperInvariant(), Description = "Apartment President - Tier 1 approval authority" },
                    new() { Name = SystemRoles.Secretary, NormalizedName = SystemRoles.Secretary.ToUpperInvariant(), Description = "Apartment Secretary - Operations and Tier 2 approval" },
                    new() { Name = SystemRoles.Treasurer, NormalizedName = SystemRoles.Treasurer.ToUpperInvariant(), Description = "Apartment Treasurer - Financial and Tier 3 approval" },
                    new() { Name = SystemRoles.MaintenanceStaff, NormalizedName = SystemRoles.MaintenanceStaff.ToUpperInvariant(), Description = "Field maintenance and asset work" },
                    new() { Name = SystemRoles.Resident, NormalizedName = SystemRoles.Resident.ToUpperInvariant(), Description = "Resident and apartment flat owner portal access" }
                };

                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            var adminRole = await context.Roles.FirstAsync(r => r.Name == SystemRoles.SystemAdmin);
            var presidentRole = await context.Roles.FirstAsync(r => r.Name == SystemRoles.President);
            var secretaryRole = await context.Roles.FirstAsync(r => r.Name == SystemRoles.Secretary);
            var treasurerRole = await context.Roles.FirstAsync(r => r.Name == SystemRoles.Treasurer);
            var staffRole = await context.Roles.FirstAsync(r => r.Name == SystemRoles.MaintenanceStaff);
            var residentRole = await context.Roles.FirstAsync(r => r.Name == SystemRoles.Resident);

            // 2. Seed Users
            if (!await context.Users.AnyAsync())
            {
                logger.LogInformation("Seeding initial administrative, committee, and resident users...");
                var defaultHash = passwordHasher.HashPassword("Admin@123456");

                var admin = new User
                {
                    Email = "admin@apartment.com",
                    NormalizedEmail = "ADMIN@APARTMENT.COM",
                    PasswordHash = defaultHash,
                    FirstName = "System",
                    LastName = "Administrator",
                    PhoneNumber = "+91 9876543210",
                    IsActive = true
                };

                var president = new User
                {
                    Email = "president@apartment.com",
                    NormalizedEmail = "PRESIDENT@APARTMENT.COM",
                    PasswordHash = passwordHasher.HashPassword("President@123456"),
                    FirstName = "Arthur",
                    LastName = "Pendleton",
                    PhoneNumber = "+91 9876543211",
                    IsActive = true
                };

                var secretary = new User
                {
                    Email = "secretary@apartment.com",
                    NormalizedEmail = "SECRETARY@APARTMENT.COM",
                    PasswordHash = passwordHasher.HashPassword("Secretary@123456"),
                    FirstName = "Beatrice",
                    LastName = "Vance",
                    PhoneNumber = "+91 9876543212",
                    IsActive = true
                };

                var treasurer = new User
                {
                    Email = "treasurer@apartment.com",
                    NormalizedEmail = "TREASURER@APARTMENT.COM",
                    PasswordHash = passwordHasher.HashPassword("Treasurer@123456"),
                    FirstName = "Charles",
                    LastName = "Montgomery",
                    PhoneNumber = "+91 9876543213",
                    IsActive = true
                };

                var staff = new User
                {
                    Email = "staff@apartment.com",
                    NormalizedEmail = "STAFF@APARTMENT.COM",
                    PasswordHash = passwordHasher.HashPassword("Staff@123456"),
                    FirstName = "David",
                    LastName = "Miller",
                    PhoneNumber = "+91 9876543214",
                    IsActive = true
                };

                var residentUser = new User
                {
                    Email = "resident@apartment.com",
                    NormalizedEmail = "RESIDENT@APARTMENT.COM",
                    PasswordHash = passwordHasher.HashPassword("Resident@123456"),
                    FirstName = "Emma",
                    LastName = "Watson",
                    PhoneNumber = "+91 9876543215",
                    IsActive = true
                };

                await context.Users.AddRangeAsync(admin, president, secretary, treasurer, staff, residentUser);
                await context.SaveChangesAsync();

                // Role mappings
                context.UserRoles.AddRange(
                    new UserRoleMapping { UserId = admin.Id, RoleId = adminRole.Id },
                    new UserRoleMapping { UserId = president.Id, RoleId = presidentRole.Id },
                    new UserRoleMapping { UserId = secretary.Id, RoleId = secretaryRole.Id },
                    new UserRoleMapping { UserId = treasurer.Id, RoleId = treasurerRole.Id },
                    new UserRoleMapping { UserId = staff.Id, RoleId = staffRole.Id },
                    new UserRoleMapping { UserId = residentUser.Id, RoleId = residentRole.Id }
                );

                // Committee Memberships
                context.CommitteeMembers.AddRange(
                    new CommitteeMember
                    {
                        UserId = president.Id,
                        CommitteeRole = SystemRoles.President,
                        TermStartDateUtc = DateTime.UtcNow.AddMonths(-3),
                        TermEndDateUtc = DateTime.UtcNow.AddMonths(21),
                        IsCurrentActive = true,
                        Remarks = "Elected for 2026-2028 tenure"
                    },
                    new CommitteeMember
                    {
                        UserId = secretary.Id,
                        CommitteeRole = SystemRoles.Secretary,
                        TermStartDateUtc = DateTime.UtcNow.AddMonths(-3),
                        TermEndDateUtc = DateTime.UtcNow.AddMonths(21),
                        IsCurrentActive = true,
                        Remarks = "Elected for 2026-2028 tenure"
                    },
                    new CommitteeMember
                    {
                        UserId = treasurer.Id,
                        CommitteeRole = SystemRoles.Treasurer,
                        TermStartDateUtc = DateTime.UtcNow.AddMonths(-3),
                        TermEndDateUtc = DateTime.UtcNow.AddMonths(21),
                        IsCurrentActive = true,
                        Remarks = "Elected for 2026-2028 tenure"
                    }
                );

                await context.SaveChangesAsync();
            }

            // 3. Seed Towers & Flats
            if (!await context.Towers.AnyAsync())
            {
                logger.LogInformation("Seeding towers, flats, and master structures...");
                var towerA = new Tower
                {
                    TowerCode = "T-A",
                    TowerName = "Tower A - Maple Crest",
                    NumberOfFloors = 14,
                    TotalFlats = 56,
                    Description = "East facing luxury residential tower with scenic park view"
                };

                var towerB = new Tower
                {
                    TowerCode = "T-B",
                    TowerName = "Tower B - Oak Ridge",
                    NumberOfFloors = 14,
                    TotalFlats = 56,
                    Description = "West facing residential tower with clubhouse access"
                };

                var towerC = new Tower
                {
                    TowerCode = "T-C",
                    TowerName = "Tower C - Pine View",
                    NumberOfFloors = 10,
                    TotalFlats = 40,
                    Description = "South residential wing with direct boulevard access"
                };

                await context.Towers.AddRangeAsync(towerA, towerB, towerC);
                await context.SaveChangesAsync();

                // Flats in Tower A
                var flats = new List<Flat>
                {
                    new() { TowerId = towerA.Id, FlatNumber = "101", FloorNumber = 1, AreaSqFt = 1850m, OwnershipStatus = FlatOwnershipStatus.Sold, OccupancyStatus = FlatOccupancyStatus.SelfOccupied },
                    new() { TowerId = towerA.Id, FlatNumber = "102", FloorNumber = 1, AreaSqFt = 1650m, OwnershipStatus = FlatOwnershipStatus.Sold, OccupancyStatus = FlatOccupancyStatus.TenantOccupied },
                    new() { TowerId = towerA.Id, FlatNumber = "201", FloorNumber = 2, AreaSqFt = 1850m, OwnershipStatus = FlatOwnershipStatus.Sold, OccupancyStatus = FlatOccupancyStatus.SelfOccupied },
                    new() { TowerId = towerA.Id, FlatNumber = "202", FloorNumber = 2, AreaSqFt = 1650m, OwnershipStatus = FlatOwnershipStatus.Sold, OccupancyStatus = FlatOccupancyStatus.Vacant },
                    new() { TowerId = towerA.Id, FlatNumber = "301", FloorNumber = 3, AreaSqFt = 2100m, OwnershipStatus = FlatOwnershipStatus.Unsold, OccupancyStatus = FlatOccupancyStatus.Vacant },

                    // Flats in Tower B
                    new() { TowerId = towerB.Id, FlatNumber = "101", FloorNumber = 1, AreaSqFt = 1750m, OwnershipStatus = FlatOwnershipStatus.Sold, OccupancyStatus = FlatOccupancyStatus.SelfOccupied },
                    new() { TowerId = towerB.Id, FlatNumber = "102", FloorNumber = 1, AreaSqFt = 1750m, OwnershipStatus = FlatOwnershipStatus.Sold, OccupancyStatus = FlatOccupancyStatus.TenantOccupied },
                    new() { TowerId = towerB.Id, FlatNumber = "201", FloorNumber = 2, AreaSqFt = 1950m, OwnershipStatus = FlatOwnershipStatus.Sold, OccupancyStatus = FlatOccupancyStatus.SelfOccupied }
                };

                await context.Flats.AddRangeAsync(flats);
                await context.SaveChangesAsync();

                // 4. Seed Residents
                var resUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "resident@apartment.com");
                var residentEmma = new Resident
                {
                    UserId = resUser?.Id,
                    FullName = "Emma Watson",
                    Email = "resident@apartment.com",
                    MobileNumber = "+91 9876543215",
                    ResidentType = ResidentType.Owner,
                    EmergencyContactName = "John Watson",
                    EmergencyContactNumber = "+91 9876543299",
                    Status = "Active"
                };

                var residentRaj = new Resident
                {
                    FullName = "Rajesh Verma",
                    Email = "rajesh.verma@example.com",
                    MobileNumber = "+91 9811223344",
                    ResidentType = ResidentType.Tenant,
                    Status = "Active"
                };

                var residentPriya = new Resident
                {
                    FullName = "Priya Nair",
                    Email = "priya.nair@example.com",
                    MobileNumber = "+91 9822334455",
                    ResidentType = ResidentType.Owner,
                    Status = "Active"
                };

                await context.Residents.AddRangeAsync(residentEmma, residentRaj, residentPriya);
                await context.SaveChangesAsync();

                // Link ownerships and occupancies
                var flatA101 = flats.First(f => f.FlatNumber == "101" && f.TowerId == towerA.Id);
                var flatA102 = flats.First(f => f.FlatNumber == "102" && f.TowerId == towerA.Id);
                var flatB101 = flats.First(f => f.FlatNumber == "101" && f.TowerId == towerB.Id);

                context.FlatOwnerships.AddRange(
                    new FlatOwnership { FlatId = flatA101.Id, ResidentId = residentEmma.Id, OwnershipPercentage = 100m, IsPrimaryOwner = true },
                    new FlatOwnership { FlatId = flatA102.Id, ResidentId = residentPriya.Id, OwnershipPercentage = 100m, IsPrimaryOwner = true },
                    new FlatOwnership { FlatId = flatB101.Id, ResidentId = residentPriya.Id, OwnershipPercentage = 100m, IsPrimaryOwner = true }
                );

                context.FlatOccupancies.Add(
                    new FlatOccupancy { FlatId = flatA102.Id, ResidentId = residentRaj.Id, OccupancyType = ResidentType.Tenant }
                );

                // 5. Seed Parking Lots
                var parkingLots = new List<ParkingLot>
                {
                    new() { LotNumber = "P-B1-001", TowerId = towerA.Id, ParkingType = ParkingType.Covered, FloorOrBasementLevel = "Basement 1", Status = ParkingLotStatus.Allocated },
                    new() { LotNumber = "P-B1-002", TowerId = towerA.Id, ParkingType = ParkingType.Covered, FloorOrBasementLevel = "Basement 1", Status = ParkingLotStatus.Allocated },
                    new() { LotNumber = "P-B1-003", TowerId = towerA.Id, ParkingType = ParkingType.Covered, FloorOrBasementLevel = "Basement 1", Status = ParkingLotStatus.Available },
                    new() { LotNumber = "P-B1-004", TowerId = towerA.Id, ParkingType = ParkingType.Covered, FloorOrBasementLevel = "Basement 1", Status = ParkingLotStatus.Available },
                    new() { LotNumber = "P-B2-001", TowerId = towerB.Id, ParkingType = ParkingType.Covered, FloorOrBasementLevel = "Basement 2", Status = ParkingLotStatus.Allocated },
                    new() { LotNumber = "P-B2-002", TowerId = towerB.Id, ParkingType = ParkingType.Covered, FloorOrBasementLevel = "Basement 2", Status = ParkingLotStatus.Available },
                    new() { LotNumber = "P-OP-001", TowerId = towerA.Id, ParkingType = ParkingType.Open, FloorOrBasementLevel = "Ground Surface", Status = ParkingLotStatus.Available },
                    new() { LotNumber = "P-OP-002", TowerId = towerB.Id, ParkingType = ParkingType.Open, FloorOrBasementLevel = "Ground Surface", Status = ParkingLotStatus.Available }
                };

                await context.ParkingLots.AddRangeAsync(parkingLots);
                await context.SaveChangesAsync();

                // Parking Allocations
                context.ParkingAllocations.AddRange(
                    new ParkingAllocation { ParkingLotId = parkingLots[0].Id, FlatId = flatA101.Id, ResidentId = residentEmma.Id, VehicleNumber = "MH-02-CD-4567", VehicleType = "Sedan (Honda City)" },
                    new ParkingAllocation { ParkingLotId = parkingLots[1].Id, FlatId = flatA102.Id, ResidentId = residentRaj.Id, VehicleNumber = "MH-02-EF-8910", VehicleType = "Hatchback (Hyundai i20)" },
                    new ParkingAllocation { ParkingLotId = parkingLots[4].Id, FlatId = flatB101.Id, ResidentId = residentPriya.Id, VehicleNumber = "MH-02-GH-1122", VehicleType = "SUV (Toyota Fortuner)" }
                );

                await context.SaveChangesAsync();
            }

            // 6. Seed Employees
            if (!await context.Employees.AnyAsync())
            {
                logger.LogInformation("Seeding employees...");
                var staffUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "staff@apartment.com");
                var employees = new List<Employee>
                {
                    new()
                    {
                        EmployeeCode = "EMP-001",
                        UserId = staffUser?.Id,
                        FirstName = "David",
                        LastName = "Miller",
                        Designation = "Facility Manager",
                        Department = "Estate Operations",
                        MobileNumber = "+91 9876543214",
                        Email = "staff@apartment.com",
                        Address = "Apartment Staff Quarters, Wing D",
                        JoiningDate = DateTime.UtcNow.AddYears(-2),
                        EmploymentType = EmploymentType.FullTime,
                        MonthlySalary = 65000.00m,
                        EmergencyContactName = "Sarah Miller",
                        EmergencyContactNumber = "+91 9876543290",
                        Status = EmploymentStatus.Active
                    },
                    new()
                    {
                        EmployeeCode = "EMP-002",
                        FirstName = "Ramesh",
                        LastName = "Chauhan",
                        Designation = "Senior Electrical Technician",
                        Department = "Engineering & Maintenance",
                        MobileNumber = "+91 9833445566",
                        Email = "ramesh.c@apartment.com",
                        Address = "Sector 14, Navi Mumbai",
                        JoiningDate = DateTime.UtcNow.AddYears(-1),
                        EmploymentType = EmploymentType.FullTime,
                        MonthlySalary = 38000.00m,
                        EmergencyContactName = "Savitri Chauhan",
                        EmergencyContactNumber = "+91 9833445599",
                        Status = EmploymentStatus.Active
                    },
                    new()
                    {
                        EmployeeCode = "EMP-003",
                        FirstName = "Sunita",
                        LastName = "Patil",
                        Designation = "Housekeeping Supervisor",
                        Department = "Housekeeping & Hygiene",
                        MobileNumber = "+91 9844556677",
                        Email = "sunita.p@apartment.com",
                        Address = "Station Road, Vashi",
                        JoiningDate = DateTime.UtcNow.AddMonths(-8),
                        EmploymentType = EmploymentType.FullTime,
                        MonthlySalary = 32000.00m,
                        EmergencyContactName = "Ganesh Patil",
                        EmergencyContactNumber = "+91 9844556699",
                        Status = EmploymentStatus.Active
                    }
                };

                await context.Employees.AddRangeAsync(employees);
                await context.SaveChangesAsync();
            }

            // 7. Seed Vendor Categories & Vendors
            if (!await context.VendorCategories.AnyAsync())
            {
                logger.LogInformation("Seeding vendor categories and vendors...");
                var categories = new List<VendorCategory>
                {
                    new() { Name = "Lift & Elevator Maintenance", CategoryType = VendorCategoryType.LiftMaintenance, Description = "Comprehensive AMC and breakdown support for passenger and freight lifts" },
                    new() { Name = "Security & Surveillance", CategoryType = VendorCategoryType.SecurityServices, Description = "Manned guarding, biometric gates, and perimeter protection" },
                    new() { Name = "Electrical & Backup Power", CategoryType = VendorCategoryType.ElectricalMaintenance, Description = "Transformers, diesel generators, switchboards, and solar panels" },
                    new() { Name = "Plumbing & Water Management", CategoryType = VendorCategoryType.Plumbing, Description = "Water pump stations, STP/WTP treatment, and drainage lines" },
                    new() { Name = "Fire Protection & Safety", CategoryType = VendorCategoryType.FireSafety, Description = "Sprinkler arrays, smoke detectors, hydrants, and regular drill compliance" },
                    new() { Name = "Gardening & Landscaping", CategoryType = VendorCategoryType.GardeningAndLandscaping, Description = "Lawn care, tree pruning, clubhouse horticulture" }
                };

                await context.VendorCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                var liftCat = categories[0];
                var secCat = categories[1];
                var elecCat = categories[2];
                var fireCat = categories[4];

                var vendors = new List<Vendor>
                {
                    new()
                    {
                        VendorName = "Otis Elevator Co. India Ltd.",
                        CategoryId = liftCat.Id,
                        ContactPerson = "Siddharth Malhotra",
                        MobileNumber = "+91 9820011223",
                        Email = "service@otis-india.com",
                        Address = "701 Trade Star, Andheri Kurla Road, Mumbai",
                        TaxRegistrationNumber = "27AAAC0011Q1Z5",
                        PaymentTerms = "Net 30 days from invoice",
                        ServiceStatus = "Active"
                    },
                    new()
                    {
                        VendorName = "ShieldGuard Security Solutions",
                        CategoryId = secCat.Id,
                        ContactPerson = "Col. R.K. Bhalla",
                        MobileNumber = "+91 9830022334",
                        Email = "bhalla@shieldguard.in",
                        Address = "Plot 42, Belapur CBD, Navi Mumbai",
                        TaxRegistrationNumber = "27BBBC1122R1Z8",
                        PaymentTerms = "Monthly advance billing",
                        ServiceStatus = "Active"
                    },
                    new()
                    {
                        VendorName = "Cummins Power Technologies",
                        CategoryId = elecCat.Id,
                        ContactPerson = "Anand Deshmukh",
                        MobileNumber = "+91 9840033445",
                        Email = "support@cummins-power.com",
                        Address = "Kothrud Industrial Estate, Pune",
                        TaxRegistrationNumber = "27CCCC2233S1Z1",
                        PaymentTerms = "50% advance, 50% on completion",
                        ServiceStatus = "Active"
                    },
                    new()
                    {
                        VendorName = "CeaseFire Safety Systems",
                        CategoryId = fireCat.Id,
                        ContactPerson = "Meera Nair",
                        MobileNumber = "+91 9850044556",
                        Email = "compliance@ceasefire.co.in",
                        Address = "MIDC Turbhe, Navi Mumbai",
                        TaxRegistrationNumber = "27DDDD3344T1Z4",
                        PaymentTerms = "Net 15 days",
                        ServiceStatus = "Active"
                    }
                };

                await context.Vendors.AddRangeAsync(vendors);
                await context.SaveChangesAsync();

                // Vendor Contracts
                var contracts = new List<VendorContract>
                {
                    new()
                    {
                        VendorId = vendors[0].Id,
                        ContractTitle = "Annual Comprehensive Elevator Maintenance (Towers A & B)",
                        ContractNumber = "CNT-OTIS-2026-01",
                        StartDate = DateTime.UtcNow.AddMonths(-6),
                        EndDate = DateTime.UtcNow.AddMonths(6),
                        RenewalReminderDate = DateTime.UtcNow.AddMonths(5),
                        ContractValue = 360000.00m,
                        Status = ContractStatus.Active,
                        ScopeOfWork = "Monthly preventive checkup, 24x7 emergency entrapment response within 30 minutes, parts replacement included."
                    },
                    new()
                    {
                        VendorId = vendors[1].Id,
                        ContractTitle = "Manned Security & Visitor Management Guarding Service",
                        ContractNumber = "CNT-SEC-2026-02",
                        StartDate = DateTime.UtcNow.AddMonths(-2),
                        EndDate = DateTime.UtcNow.AddMonths(10),
                        RenewalReminderDate = DateTime.UtcNow.AddMonths(9),
                        ContractValue = 1440000.00m,
                        Status = ContractStatus.Active,
                        ScopeOfWork = "Deployment of 8 security guards (2 shifts of 4) plus 1 day supervisor, boom barrier maintenance."
                    }
                };

                await context.VendorContracts.AddRangeAsync(contracts);
                await context.SaveChangesAsync();
            }

            // 8. Seed Asset Categories & Assets
            if (!await context.AssetCategories.AnyAsync())
            {
                logger.LogInformation("Seeding asset categories and high-value inventory...");
                var categories = new List<AssetCategory>
                {
                    new() { Name = "Lifts & Elevators", Description = "Passenger and freight vertical transportation equipment" },
                    new() { Name = "Power Backup & Generators", Description = "Diesel generators, ATS panels, synchronizing panels" },
                    new() { Name = "Water Pumping Systems", Description = "Hydro-pneumatic pumps, borewell pumps, drainage submersibles" },
                    new() { Name = "Fire Fighting Systems", Description = "Diesel fire pumps, jockey pumps, sprinkler networks, extinguishers" },
                    new() { Name = "Security & Access", Description = "CCTV IP cameras, boom barriers, clubhouse turnstiles" }
                };

                await context.AssetCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                var towerA = await context.Towers.FirstAsync(t => t.TowerCode == "T-A");
                var otisVendor = await context.Vendors.FirstAsync(v => v.VendorName.Contains("Otis"));
                var cumminsVendor = await context.Vendors.FirstAsync(v => v.VendorName.Contains("Cummins"));

                var assets = new List<Asset>
                {
                    new()
                    {
                        AssetCode = "AST-LFT-A1",
                        AssetName = "Passenger Lift A1 (13 Passengers / 884 Kg)",
                        CategoryId = categories[0].Id,
                        Description = "Geared passenger elevator servicing G to 14th floor",
                        Manufacturer = "Otis Elevators",
                        ModelNumber = "Gen2-Core",
                        SerialNumber = "OTIS-2024-9912",
                        PurchaseDate = DateTime.UtcNow.AddYears(-2),
                        PurchaseCost = 1450000.00m,
                        WarrantyExpiryDate = DateTime.UtcNow.AddYears(-1),
                        TowerId = towerA.Id,
                        SpecificLocation = "Tower A Central Shaft",
                        AssignedVendorId = otisVendor.Id,
                        Condition = AssetCondition.Good,
                        Status = "Operational",
                        LastMaintenanceDate = DateTime.UtcNow.AddDays(-20),
                        NextScheduledMaintenanceDate = DateTime.UtcNow.AddDays(10)
                    },
                    new()
                    {
                        AssetCode = "AST-GEN-01",
                        AssetName = "500 kVA Diesel Generator Set",
                        CategoryId = categories[1].Id,
                        Description = "Emergency back-up silent generator for common areas and lifts",
                        Manufacturer = "Cummins Power Systems",
                        ModelNumber = "QSK19-G4",
                        SerialNumber = "CUM-IND-2023-45",
                        PurchaseDate = DateTime.UtcNow.AddYears(-3),
                        PurchaseCost = 2850000.00m,
                        WarrantyExpiryDate = DateTime.UtcNow.AddYears(-1),
                        TowerId = towerA.Id,
                        SpecificLocation = "Utility Courtyard Ground Level",
                        AssignedVendorId = cumminsVendor.Id,
                        Condition = AssetCondition.NeedsMaintenance,
                        Status = "Operational",
                        LastMaintenanceDate = DateTime.UtcNow.AddDays(-60),
                        NextScheduledMaintenanceDate = DateTime.UtcNow.AddDays(-5) // Overdue for routine service
                    }
                };

                await context.Assets.AddRangeAsync(assets);
                await context.SaveChangesAsync();
            }

            // 9. Seed Purchase Requests & Approvals
            if (!await context.PurchaseRequests.AnyAsync())
            {
                logger.LogInformation("Seeding purchase requests and approval workflow states...");
                var staffUser = await context.Users.FirstAsync(u => u.Email == "staff@apartment.com");
                var cumminsVendor = await context.Vendors.FirstAsync(v => v.VendorName.Contains("Cummins"));
                var genAsset = await context.Assets.FirstAsync(a => a.AssetCode == "AST-GEN-01");

                var pr1 = new PurchaseRequest
                {
                    RequestNumber = "PR-2026-0001",
                    Title = "Semi-Annual DG Overhaul & Lube Oil Filter Replacement",
                    Description = "Scheduled major preventive service for 500 kVA Cummins DG set to ensure uninterrupted monsoon power backup.",
                    RequesterId = staffUser.Id,
                    VendorId = cumminsVendor.Id,
                    EstimatedTotalAmount = 45000.00m,
                    TaxAndOtherCharges = 8100.00m, // 18% GST
                    BudgetCategory = "Power & Backup Maintenance",
                    RequiredDeliveryDate = DateTime.UtcNow.AddDays(15),
                    Status = PurchaseRequestStatus.PendingApproval,
                    CurrentApprovalStep = 1,
                    CurrentAssignedRole = SystemRoles.President
                };

                pr1.Items.Add(new PurchaseRequestItem
                {
                    ItemDescription = "Cummins High Performance Engine Oil (Valvoline Premium Blue 15W-40) - 50 Liters",
                    Quantity = 1,
                    UnitOfMeasure = "Drum",
                    EstimatedUnitPrice = 24000.00m,
                    RelatedAssetId = genAsset.Id
                });

                pr1.Items.Add(new PurchaseRequestItem
                {
                    ItemDescription = "Fleetguard Lubricant & Fuel Filters Kit (Set of 3)",
                    Quantity = 2,
                    UnitOfMeasure = "Kit",
                    EstimatedUnitPrice = 10500.00m,
                    RelatedAssetId = genAsset.Id
                });

                await context.PurchaseRequests.AddAsync(pr1);
                await context.SaveChangesAsync();
            }

            logger.LogInformation("Database initialization and master data seeding successfully completed!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database seeding.");
            throw;
        }
    }
}
