using _10xVibeTravels.Data;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Interfaces;
using _10xVibeTravels.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace _10xVibeTravels.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(
            IDbContextFactory<ApplicationDbContext> contextFactory, 
            UserManager<ApplicationUser> userManager,
            ILogger<UserProfileService> logger)
        {
            _contextFactory = contextFactory;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var userProfile = await context.UserProfiles
                .AsNoTracking()
                .Include(up => up.TravelStyle)
                .Include(up => up.Intensity)
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null)
            {
                _logger.LogWarning("User profile not found for user ID: {UserId}", userId);
                return null;
            }

            var userInterests = await context.UserInterests
                .AsNoTracking()
                .Where(ui => ui.UserId == userId)
                .Include(ui => ui.Interest)
                .Select(ui => new LookupDto { Id = ui.InterestId, Name = ui.Interest!.Name })
                .ToListAsync();

            return MapToUserProfileDto(userProfile, userInterests);
        }

        public async Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileCommand command)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var userProfile = await context.UserProfiles
                .Include(up => up.TravelStyle)
                .Include(up => up.Intensity)
                .FirstOrDefaultAsync(up => up.UserId == userId);

            bool isNewProfile = false;
            if (userProfile == null)
            {
                isNewProfile = true;
                _logger.LogInformation("Creating new user profile for user ID: {UserId}", userId);
                userProfile = new UserProfile { UserId = userId };
                context.UserProfiles.Add(userProfile);
            }

            Guid? originalTravelStyleId = userProfile.TravelStyleId;
            Guid? originalIntensityId = userProfile.IntensityId;

            if (command.TravelStyleId.HasValue && !await context.TravelStyles.AnyAsync(ts => ts.Id == command.TravelStyleId.Value))
            {
                throw new KeyNotFoundException($"TravelStyle with ID {command.TravelStyleId} not found.");
            }
            if (command.IntensityId.HasValue && !await context.Intensities.AnyAsync(i => i.Id == command.IntensityId.Value))
            {
                throw new KeyNotFoundException($"Intensity with ID {command.IntensityId} not found.");
            }

            userProfile.Budget = command.Budget;
            userProfile.TravelStyleId = command.TravelStyleId;
            userProfile.IntensityId = command.IntensityId;
            userProfile.ModifiedAt = DateTime.UtcNow;

            try
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully saved profile changes for UserId: {UserId}. Is new profile: {IsNewProfile}", userId, isNewProfile);
                
                bool travelStyleChanged = originalTravelStyleId != userProfile.TravelStyleId;
                bool intensityChanged = originalIntensityId != userProfile.IntensityId;

                if (isNewProfile || travelStyleChanged)
                    await context.Entry(userProfile).Reference(up => up.TravelStyle).LoadAsync();
                if (isNewProfile || intensityChanged)
                    await context.Entry(userProfile).Reference(up => up.Intensity).LoadAsync();
                
                var userInterests = await context.UserInterests
                    .AsNoTracking()
                    .Where(ui => ui.UserId == userId)
                    .Include(ui => ui.Interest)
                    .Select(ui => new LookupDto { Id = ui.InterestId, Name = ui.Interest!.Name })
                    .ToListAsync();

                return MapToUserProfileDto(userProfile, userInterests);
            }
            catch (DbUpdateException ex)
            {                
                _logger.LogError(ex, "Error updating user profile for user ID: {UserId}", userId);
                throw new InvalidOperationException("An error occurred while updating the profile. Please check the provided IDs.", ex);
            }
        }

        public async Task<List<LookupDto>> SetUserInterestsAsync(string userId, SetUserInterestsCommand command)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var userExists = await context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            var validInterestIds = await context.Interests
                .Where(i => command.InterestIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync();
            
            var invalidIds = command.InterestIds.Except(validInterestIds).ToList();
            if (invalidIds.Any())
            {   
                _logger.LogWarning("Invalid interest IDs provided for user {UserId}: {InvalidIds}", userId, string.Join(", ", invalidIds));
                throw new KeyNotFoundException($"Invalid interest IDs provided: {string.Join(", ", invalidIds)}");
            }

            var currentUserInterests = await context.UserInterests
                .Where(ui => ui.UserId == userId)
                .ToListAsync();

            var interestsToRemove = currentUserInterests
                .Where(ui => !command.InterestIds.Contains(ui.InterestId))
                .ToList(); 
            
            if (interestsToRemove.Any())
            {
                context.UserInterests.RemoveRange(interestsToRemove);
            }

            var existingInterestIds = currentUserInterests.Select(ui => ui.InterestId).ToList();
            var interestsToAdd = command.InterestIds
                .Where(id => !existingInterestIds.Contains(id))
                .Select(id => new UserInterest { UserId = userId, InterestId = id })
                .ToList();

            if (interestsToAdd.Any())
            {
                context.UserInterests.AddRange(interestsToAdd);
            }

            if (interestsToRemove.Any() || interestsToAdd.Any())
            {
                try
                {                    
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Successfully updated interests for user ID: {UserId}", userId);
                }
                catch (DbUpdateException ex)
                {                    
                    _logger.LogError(ex, "Error setting user interests for user ID: {UserId}", userId);
                    throw new InvalidOperationException("An error occurred while updating interests.", ex);
                }
            }
            else
            {
                _logger.LogInformation("No changes to user interests for user ID: {UserId}", userId);
            }
            
            var finalInterestLookups = await context.Interests
                .Where(i => command.InterestIds.Contains(i.Id))
                .Select(i => new LookupDto { Id = i.Id, Name = i.Name })
                .ToListAsync();

            return finalInterestLookups;
        }

        private static UserProfileDto MapToUserProfileDto(UserProfile userProfile, List<LookupDto> interests)
        {
            return new UserProfileDto
            {
                Budget = userProfile.Budget,
                TravelStyle = userProfile.TravelStyle == null ? null : new LookupDto
                {
                    Id = userProfile.TravelStyleId!.Value,
                    Name = userProfile.TravelStyle.Name
                },
                Intensity = userProfile.Intensity == null ? null : new LookupDto
                {
                    Id = userProfile.IntensityId!.Value,
                    Name = userProfile.Intensity.Name
                },
                Interests = interests,
                CreatedAt = userProfile.CreatedAt,
                ModifiedAt = userProfile.ModifiedAt
            };
        }
    }
} 