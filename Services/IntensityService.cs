using _10xVibeTravels.Data;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _10xVibeTravels.Services
{
    public class IntensityService : IIntensityService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public IntensityService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<IntensityDto>> GetAllAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Intensities
                .AsNoTracking()
                .Select(i => new IntensityDto { Id = i.Id, Name = i.Name })
                .ToListAsync();
        }
    }
} 