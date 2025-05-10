using System.ComponentModel.DataAnnotations;

namespace _10xVibeTravels.Requests
{
    public class SetUserInterestsCommand
    {
        [Required] // Plan specifies non-null
        public List<Guid> InterestIds { get; set; } = new List<Guid>();
    }
} 