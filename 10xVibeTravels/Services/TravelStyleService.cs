using _10xVibeTravels.Data;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace _10xVibeTravels.Services
{
    public class TravelStyleService : ITravelStyleService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public TravelStyleService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<TravelStyleDto>> GetAllAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.TravelStyles
                .AsNoTracking()
                .Select(ts => new TravelStyleDto { Id = ts.Id, Name = ts.Name })
                .ToListAsync();
        }
    }
} 