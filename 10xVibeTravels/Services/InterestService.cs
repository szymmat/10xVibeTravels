using _10xVibeTravels.Data;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _10xVibeTravels.Services
{
    public class InterestService : IInterestService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public InterestService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<InterestDto>> GetAllAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Interests
                .AsNoTracking()
                .Select(i => new InterestDto { Id = i.Id, Name = i.Name })
                .ToListAsync();
        }
    }
} 