using FluentValidation;
using _10xVibeTravels.Requests;

namespace _10xVibeTravels.Validators.Notes;

public class GetNotesListQueryValidator : AbstractValidator<GetNotesListQuery>
{
    private readonly string[] _allowedSortByValues = { "createdat", "modifiedat" };
    private readonly string[] _allowedSortDirections = { "asc", "desc" };

    public GetNotesListQueryValidator()
    {
        RuleFor(q => q.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1.");

        RuleFor(q => q.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize must be greater than or equal to 1.")
            .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100."); // Sensible upper limit

        RuleFor(q => q.SortBy)
            .Must(value => string.IsNullOrEmpty(value) || _allowedSortByValues.Contains(value.ToLowerInvariant()))
            .WithMessage($"SortBy must be one of the following: {string.Join(", ", _allowedSortByValues)} or empty.");

        RuleFor(q => q.SortDirection)
            .Must(value => string.IsNullOrEmpty(value) || _allowedSortDirections.Contains(value.ToLowerInvariant()))
            .WithMessage($"SortDirection must be one of the following: {string.Join(", ", _allowedSortDirections)} or empty.");
    }
} 