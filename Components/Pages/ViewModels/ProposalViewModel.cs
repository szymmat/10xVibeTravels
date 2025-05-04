using System.ComponentModel.DataAnnotations;
using _10xVibeTravels.Data;

namespace _10xVibeTravels.Components.Pages.ViewModels;

public class ProposalViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
    public string Title { get; set; } = string.Empty;

    public string OriginalTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public PlanStatus Status { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal Budget { get; set; }

    // UI State
    public bool IsEditingTitle { get; set; } = false;
    public bool IsLoadingAction { get; set; } = false;
    public string? ErrorMessage { get; set; }
} 