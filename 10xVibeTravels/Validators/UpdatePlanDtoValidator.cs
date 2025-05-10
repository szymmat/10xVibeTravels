using _10xVibeTravels.Data; // For PlanStatus enum
using _10xVibeTravels.Dtos; // For UpdatePlanDto
using FluentValidation;

namespace _10xVibeTravels.Validators;

public class UpdatePlanDtoValidator : AbstractValidator<UpdatePlanDto>
{
    public UpdatePlanDtoValidator()
    {
        // Rule for Title: MaxLength(100)
        RuleFor(x => x.Title)
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters.")
            .When(x => x.Title != null); // Only apply if Title is provided

        // Rule for Status: Must be Accepted or Rejected if provided
        RuleFor(x => x.Status)
            .Must(status => status == PlanStatus.Accepted || status == PlanStatus.Rejected)
            .WithMessage("Status can only be set to Accepted or Rejected.")
            .When(x => x.Status.HasValue); // Only apply if Status is provided

        // Rule: At least one property must be non-null for PATCH
        RuleFor(x => x)
            .Must(dto => dto.Status.HasValue || dto.Title != null || dto.Content != null)
            .WithMessage("At least one field (Status, Title, or Content) must be provided for update.");

        // Note: The complex validation (e.g., cannot update Content if status is Generated)
        // is handled in the PlanService as it requires fetching the current state of the Plan.
    }
} 