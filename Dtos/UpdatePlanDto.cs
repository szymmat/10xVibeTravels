using _10xVibeTravels.Data;
using System.ComponentModel.DataAnnotations;

namespace _10xVibeTravels.Dtos; // Namespace Dtos

public class UpdatePlanDto
{
    /// <summary>
    /// New status for the plan (only if current status is "Generated"). Allowed values: "Accepted", "Rejected".
    /// </summary>
    public PlanStatus? Status { get; set; }

    /// <summary>
    /// New title for the plan (only if current status is "Generated"). Max 100 characters.
    /// </summary>
    [MaxLength(100)]
    public string? Title { get; set; }

    /// <summary>
    /// New content for the plan (only if current status is "Accepted").
    /// </summary>
    public string? Content { get; set; }
} 