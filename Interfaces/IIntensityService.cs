using _10xVibeTravels.Dtos;

namespace _10xVibeTravels.Interfaces
{
    public interface IIntensityService
    {
        Task<IEnumerable<IntensityDto>> GetAllAsync();
    }
} 