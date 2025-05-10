using System.ComponentModel.DataAnnotations;

namespace _10xVibeTravels.Requests;

public class GeneratePlanProposalRequest
{
    [Required]
    public Guid NoteId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Range(0, (double)decimal.MaxValue)]
    public decimal? Budget { get; set; }
}