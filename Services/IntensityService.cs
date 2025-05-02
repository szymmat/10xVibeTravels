using _10xVibeTravels.Data;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace _10xVibeTravels.Services
{
    public class IntensityService : IIntensityService
    {
        private readonly ApplicationDbContext _context;

        public IntensityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IntensityDto>> GetAllAsync()
        {
            var intensities = await _context.Intensities.ToListAsync();
            
            // TODO: Replace manual mapping with AutoMapper
            return intensities.Select(i => new IntensityDto
            {
                Id = i.Id,
                Name = i.Name
            });
        }
    }
} 