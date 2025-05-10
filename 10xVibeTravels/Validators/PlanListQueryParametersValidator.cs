using _10xVibeTravels.Requests;
using FluentValidation;

namespace _10xVibeTravels.Validators;

public class PlanListQueryParametersValidator : AbstractValidator<PlanListQueryParameters>
{
    private static readonly string[] AllowedStatusValues = { "Generated", "Accepted", "Rejected" };
    private static readonly string[] AllowedSortByValues = { "createdAt", "modifiedAt" };
    private static readonly string[] AllowedSortDirectionValues = { "asc", "desc" };

    public PlanListQueryParametersValidator()
    {
        RuleFor(x => x.Status)
            .Must(status => AllowedStatusValues.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of the following: {string.Join(", ", AllowedStatusValues)}.");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.SortBy)
            .Must(sortBy => AllowedSortByValues.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"SortBy must be one of the following: {string.Join(", ", AllowedSortByValues)}.");

        RuleFor(x => x.SortDirection)
            .Must(dir => AllowedSortDirectionValues.Contains(dir, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"SortDirection must be one of the following: {string.Join(", ", AllowedSortDirectionValues)}.");
    }
} 