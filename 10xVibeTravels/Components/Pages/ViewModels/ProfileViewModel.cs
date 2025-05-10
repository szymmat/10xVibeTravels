using System.ComponentModel.DataAnnotations;

namespace _10xVibeTravels.Components.Pages.ViewModels;

public class ProfileViewModel
{
    [Range(0, double.MaxValue, ErrorMessage = "Budżet nie może być ujemny.")]
    public decimal? Budget { get; set; }
    public Guid? TravelStyleId { get; set; }
    public Guid? IntensityId { get; set; }
    public List<Guid> Interests { get; set; } = new List<Guid>();
} 