using _10xVibeTravels.Dtos;

namespace _10xVibeTravels.Interfaces
{
    public interface IInterestService
    {
        Task<IEnumerable<InterestDto>> GetAllAsync();
    }
} 