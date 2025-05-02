using _10xVibeTravels.Data;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace _10xVibeTravels.Services
{
    public class InterestService : IInterestService
    {
        private readonly ApplicationDbContext _context;

        public InterestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InterestDto>> GetAllAsync()
        {
            var interests = await _context.Interests.ToListAsync();

            // TODO: Replace manual mapping with AutoMapper
            return interests.Select(i => new InterestDto
            {
                Id = i.Id,
                Name = i.Name
            });
        }
    }
} 