using System.ComponentModel.DataAnnotations;

namespace _10xVibeTravels.Requests; // Updated namespace

public class PlanListQueryParameters
{
    [AllowedValues("Generated", "Accepted", "Rejected", ErrorMessage = "Invalid status value.")]
    public string Status { get; set; } = "Accepted"; // Default as per plan

    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")] // Max PageSize set to 100 for safety
    public int PageSize { get; set; } = 10;

    [AllowedValues("createdAt", "modifiedAt", ErrorMessage = "Invalid sortBy value.")]
    public string SortBy { get; set; } = "modifiedAt";

    [AllowedValues("asc", "desc", ErrorMessage = "Invalid sortDirection value.")]
    public string SortDirection { get; set; } = "desc";

    // Helper to convert string status to PlanStatus enum if needed, although service layer might handle this.
    // Could also use a custom model binder.
    // public PlanStatus? GetPlanStatusEnum()
    // {
    //     if (Enum.TryParse<PlanStatus>(Status, true, out var statusEnum))
    //     {
    //         return statusEnum;
    //     }
    //     return null;
    // }
}

// Helper attribute for allowed string values (can be moved to a shared location)
public class AllowedValuesAttribute : ValidationAttribute
{
    private readonly string[] _allowedValues;

    public AllowedValuesAttribute(params string[] allowedValues)
    {
        _allowedValues = allowedValues;
    }

    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string stringValue)
        {
            if (_allowedValues.Contains(stringValue, StringComparer.OrdinalIgnoreCase))
            {
                return ValidationResult.Success!;
            }
            return new ValidationResult(ErrorMessage ?? $"The value must be one of the following: {string.Join(", ", _allowedValues)}.");
        }
        return ValidationResult.Success!; // Allow null or other types if needed, or adjust logic
    }
} 