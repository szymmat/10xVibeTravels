using _10xVibeTravels.Data;

namespace _10xVibeTravels.Responses; // Updated namespace

public class PlanListItemDto
{
    public Guid Id { get; set; }
    public PlanStatus Status { get; set; }
    public required string Title { get; set; }
    public required string ContentPreview { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal? Budget { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
} 