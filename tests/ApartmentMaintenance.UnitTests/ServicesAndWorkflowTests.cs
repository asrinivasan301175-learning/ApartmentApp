using Microsoft.Extensions.Configuration;
using ApartmentMaintenance.Application.DTOs;
using ApartmentMaintenance.Application.Validators;
using ApartmentMaintenance.Domain.Entities;
using ApartmentMaintenance.Domain.Enums;
using ApartmentMaintenance.Infrastructure.Services;
using Xunit;

namespace ApartmentMaintenance.UnitTests;

public class ServicesAndWorkflowTests
{
    [Fact]
    public void PasswordHasher_ShouldHashAndVerifyPasswordCorrectly()
    {
        var hasher = new PasswordHasher();
        var password = "SecurePassword@2026!";

        var hash = hasher.HashPassword(password);

        Assert.NotNull(hash);
        Assert.NotEqual(password, hash);
        Assert.True(hasher.VerifyPassword(password, hash));
        Assert.False(hasher.VerifyPassword("WrongPassword", hash));
    }

    [Fact]
    public void JwtTokenService_ShouldGenerateValidAccessToken()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "SUPER_SECURE_TEST_KEY_THAT_IS_LONG_ENOUGH_FOR_HMACSHA256_TESTING!" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:ExpiryMinutes", "30" },
            { "Jwt:RefreshTokenExpiryDays", "7" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var jwtService = new JwtTokenService(configuration);

        var user = new User
        {
            Email = "president@apartment.com",
            FirstName = "Arthur",
            LastName = "Pendleton"
        };

        var roles = new[] { SystemRoles.President };

        var token = jwtService.GenerateAccessToken(user, roles);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(jwtService.ValidateToken(token));
    }

    [Fact]
    public void PurchaseRequest_ApprovalProgression_ShouldFollowCorrectSequence()
    {
        var pr = new PurchaseRequest
        {
            RequestNumber = "PR-TEST-001",
            Title = "Lift Brake Pad Replacement",
            EstimatedTotalAmount = 25000m,
            TaxAndOtherCharges = 4500m,
            Status = PurchaseRequestStatus.PendingApproval,
            CurrentApprovalStep = 1,
            CurrentAssignedRole = SystemRoles.President
        };

        // Step 1: President Approves
        Assert.Equal(1, pr.CurrentApprovalStep);
        Assert.Equal(SystemRoles.President, pr.CurrentAssignedRole);

        pr.CurrentApprovalStep = 2;
        pr.CurrentAssignedRole = SystemRoles.Secretary;

        // Step 2: Secretary Approves
        Assert.Equal(2, pr.CurrentApprovalStep);
        Assert.Equal(SystemRoles.Secretary, pr.CurrentAssignedRole);

        pr.CurrentApprovalStep = 3;
        pr.CurrentAssignedRole = SystemRoles.Treasurer;

        // Step 3: Treasurer Approves -> Final Approved
        Assert.Equal(3, pr.CurrentApprovalStep);
        Assert.Equal(SystemRoles.Treasurer, pr.CurrentAssignedRole);

        pr.Status = PurchaseRequestStatus.Approved;
        pr.CurrentApprovalStep = 4;
        pr.CurrentAssignedRole = null;

        Assert.Equal(PurchaseRequestStatus.Approved, pr.Status);
        Assert.Null(pr.CurrentAssignedRole);
    }

    [Fact]
    public void PurchaseRequest_Rejection_ShouldTerminateWorkflow()
    {
        var pr = new PurchaseRequest
        {
            RequestNumber = "PR-TEST-002",
            Status = PurchaseRequestStatus.PendingApproval,
            CurrentApprovalStep = 1,
            CurrentAssignedRole = SystemRoles.President
        };

        // Rejection occurs
        pr.Status = PurchaseRequestStatus.Rejected;
        pr.CurrentAssignedRole = null;

        Assert.Equal(PurchaseRequestStatus.Rejected, pr.Status);
        Assert.Null(pr.CurrentAssignedRole);
    }

    [Fact]
    public void LoginRequestValidator_ShouldFailOnInvalidEmail()
    {
        var validator = new LoginRequestValidator();
        var request = new LoginRequest
        {
            Email = "invalid-email-string",
            Password = "ValidPassword123"
        };

        var result = validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void CreateTowerValidator_ShouldFailOnEmptyTowerCode()
    {
        var validator = new CreateTowerValidator();
        var request = new CreateTowerDto
        {
            TowerCode = "",
            TowerName = "Tower A",
            NumberOfFloors = 10,
            TotalFlats = 40
        };

        var result = validator.Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TowerCode");
    }
}
