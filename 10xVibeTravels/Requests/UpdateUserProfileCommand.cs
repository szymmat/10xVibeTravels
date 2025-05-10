namespace _10xVibeTravels.Requests
{
    public class UpdateUserProfileCommand
    {
        public decimal? Budget { get; set; }
        public Guid? TravelStyleId { get; set; }
        public Guid? IntensityId { get; set; }
    }
} 