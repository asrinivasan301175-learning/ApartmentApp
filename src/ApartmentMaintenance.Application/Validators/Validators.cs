using FluentValidation;
using ApartmentMaintenance.Application.DTOs;

namespace ApartmentMaintenance.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}

public class CreateTowerValidator : AbstractValidator<CreateTowerDto>
{
    public CreateTowerValidator()
    {
        RuleFor(x => x.TowerCode)
            .NotEmpty().WithMessage("Tower code is required.")
            .MaximumLength(20).WithMessage("Tower code cannot exceed 20 characters.");

        RuleFor(x => x.TowerName)
            .NotEmpty().WithMessage("Tower name is required.")
            .MaximumLength(100).WithMessage("Tower name cannot exceed 100 characters.");

        RuleFor(x => x.NumberOfFloors)
            .GreaterThan(0).WithMessage("Number of floors must be greater than zero.");

        RuleFor(x => x.TotalFlats)
            .GreaterThan(0).WithMessage("Total flats must be greater than zero.");
    }
}

public class CreateFlatValidator : AbstractValidator<CreateFlatDto>
{
    public CreateFlatValidator()
    {
        RuleFor(x => x.TowerId)
            .GreaterThan(0).WithMessage("Valid Tower is required.");

        RuleFor(x => x.FlatNumber)
            .NotEmpty().WithMessage("Flat number is required.")
            .MaximumLength(20).WithMessage("Flat number cannot exceed 20 characters.");

        RuleFor(x => x.FloorNumber)
            .GreaterThanOrEqualTo(0).WithMessage("Floor number must be 0 (ground) or greater.");

        RuleFor(x => x.AreaSqFt)
            .GreaterThan(0).WithMessage("Area in square feet must be greater than zero.");
    }
}

public class CreatePurchaseRequestValidator : AbstractValidator<CreatePurchaseRequestDto>
{
    public CreatePurchaseRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Purchase request title is required.")
            .MaximumLength(150).WithMessage("Title cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.");

        RuleFor(x => x.BudgetCategory)
            .NotEmpty().WithMessage("Budget category is required.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one line item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemDescription).NotEmpty().WithMessage("Item description is required.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be at least 1.");
            item.RuleFor(i => i.EstimatedUnitPrice).GreaterThan(0).WithMessage("Estimated unit price must be greater than zero.");
        });
    }
}
