using _10xVibeTravels.Dtos;

namespace _10xVibeTravels.Interfaces
{
    public interface ITravelStyleService
    {
        Task<IEnumerable<TravelStyleDto>> GetAllAsync();
    }
} 