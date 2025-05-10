namespace _10xVibeTravels.Dtos
{
    public class UserProfileDto
    {
        public decimal? Budget { get; set; }
        public LookupDto? TravelStyle { get; set; }
        public LookupDto? Intensity { get; set; }
        public List<LookupDto> Interests { get; set; } = new List<LookupDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
} 