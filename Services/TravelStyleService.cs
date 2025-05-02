using _10xVibeTravels.Data;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace _10xVibeTravels.Services
{
    public class TravelStyleService : ITravelStyleService
    {
        private readonly ApplicationDbContext _context;

        public TravelStyleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TravelStyleDto>> GetAllAsync()
        {
            var travelStyles = await _context.TravelStyles.ToListAsync();
            
            // TODO: Replace manual mapping with AutoMapper
            return travelStyles.Select(t => new TravelStyleDto
            {
                Id = t.Id,
                Name = t.Name
            });
        }
    }
} 