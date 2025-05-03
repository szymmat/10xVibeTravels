using _10xVibeTravels.Data;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Interfaces;
using _10xVibeTravels.Requests;
using Microsoft.EntityFrameworkCore;

namespace _10xVibeTravels.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(ApplicationDbContext context, ILogger<UserProfileService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            _logger.LogInformation("Fetching user profile core data for UserId: {UserId}", userId);

            // Fetch Profile without Interests first
            var userProfile = await _context.UserProfiles
                .AsNoTracking()
                .Include(up => up.TravelStyle)
                .Include(up => up.Intensity)
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userProfile == null)
            {
                _logger.LogWarning("User profile not found for UserId: {UserId}", userId);
                return null; // As per plan, return null if not found for GET
            }

            _logger.LogInformation("Fetching user interests for UserId: {UserId}", userId);
            
            // Fetch Interests separately using the UserId
            var userInterests = await _context.UserInterests
                .AsNoTracking()
                .Where(ui => ui.UserId == userId)
                .Include(ui => ui.Interest)
                .Select(ui => new LookupDto
                {
                    Id = ui.Interest!.Id,
                    Name = ui.Interest.Name
                })
                .ToListAsync(); // Fetch the interests

            // Manual Mapping (Combine profile and interests)
            var userProfileDto = new UserProfileDto
            {
                Budget = userProfile.Budget,
                TravelStyle = userProfile.TravelStyle == null ? null : new LookupDto
                {
                    Id = userProfile.TravelStyle.Id,
                    Name = userProfile.TravelStyle.Name
                },
                Intensity = userProfile.Intensity == null ? null : new LookupDto
                {
                    Id = userProfile.Intensity.Id,
                    Name = userProfile.Intensity.Name
                },
                Interests = userInterests, // Assign the fetched interests
                CreatedAt = userProfile.CreatedAt,
                ModifiedAt = userProfile.ModifiedAt
            };

            _logger.LogInformation("Successfully fetched user profile and interests for UserId: {UserId}", userId);
            return userProfileDto;
        }

        public async Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileCommand command)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            _logger.LogInformation("Attempting to update user profile for UserId: {UserId}", userId);

            var userProfile = await _context.UserProfiles
                // Tracking is needed for updates
                .Include(up => up.TravelStyle) // Include for mapping response
                .Include(up => up.Intensity)   // Include for mapping response
                .FirstOrDefaultAsync(up => up.UserId == userId);

            var isNewProfile = false;
            if (userProfile == null)
            {
                _logger.LogInformation("No existing profile found for UserId: {UserId}. Creating a new one.", userId);
                isNewProfile = true;
                userProfile = new UserProfile
                {
                    UserId = userId,
                    // Id, CreatedAt, ModifiedAt are set by database default
                };
                _context.UserProfiles.Add(userProfile);
            }

            // --- Validation --- 
            // Check if provided TravelStyleId exists
            if (command.TravelStyleId.HasValue && !await _context.TravelStyles.AnyAsync(ts => ts.Id == command.TravelStyleId.Value))
            {
                _logger.LogError("Validation failed: TravelStyleId {TravelStyleId} not found for UserId: {UserId}", command.TravelStyleId, userId);
                throw new KeyNotFoundException($"TravelStyle with ID {command.TravelStyleId} not found.");
            }

            // Check if provided IntensityId exists
            if (command.IntensityId.HasValue && !await _context.Intensities.AnyAsync(i => i.Id == command.IntensityId.Value))
            {
                 _logger.LogError("Validation failed: IntensityId {IntensityId} not found for UserId: {UserId}", command.IntensityId, userId);
                throw new KeyNotFoundException($"Intensity with ID {command.IntensityId} not found.");
            }

            // --- Update Logic --- 
            userProfile.Budget = command.Budget;
            userProfile.TravelStyleId = command.TravelStyleId;
            userProfile.IntensityId = command.IntensityId;
            userProfile.ModifiedAt = DateTime.UtcNow; // Update ModifiedAt timestamp

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully saved profile changes for UserId: {UserId}. Is new profile: {IsNewProfile}", userId, isNewProfile);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error saving profile changes for UserId: {UserId}", userId);
                // Consider more specific error handling based on exception details
                throw; // Re-throw for global exception handling
            }

            // --- Response Mapping --- 
            // We need to potentially reload the navigation properties if they were updated by Id
            // or if it's a new profile.
            // Fetching separately is cleaner than reloading the whole entity.

            _logger.LogInformation("Fetching updated profile data for response for UserId: {UserId}", userId);

            // Fetch related data for the response DTO
            var updatedTravelStyle = command.TravelStyleId.HasValue
                ? await _context.TravelStyles.AsNoTracking().FirstOrDefaultAsync(ts => ts.Id == command.TravelStyleId.Value)
                : null;
            var updatedIntensity = command.IntensityId.HasValue
                ? await _context.Intensities.AsNoTracking().FirstOrDefaultAsync(i => i.Id == command.IntensityId.Value)
                : null;

            // Fetch current interests (unchanged by this method)
             _logger.LogInformation("Fetching user interests for response for UserId: {UserId}", userId);
            var userInterests = await _context.UserInterests
                .AsNoTracking()
                .Where(ui => ui.UserId == userId)
                .Include(ui => ui.Interest)
                .Select(ui => new LookupDto
                {
                    Id = ui.Interest!.Id,
                    Name = ui.Interest.Name
                })
                .ToListAsync();

            // Map final state to UserProfileDto
            var userProfileDto = new UserProfileDto
            {
                Budget = userProfile.Budget,
                TravelStyle = updatedTravelStyle == null ? null : new LookupDto
                {
                    Id = updatedTravelStyle.Id,
                    Name = updatedTravelStyle.Name
                },
                Intensity = updatedIntensity == null ? null : new LookupDto
                {
                    Id = updatedIntensity.Id,
                    Name = updatedIntensity.Name
                },
                Interests = userInterests, // Use the separately fetched interests
                CreatedAt = userProfile.CreatedAt, // This won't be available if newly created until AFTER SaveChanges typically, DB default handles it.
                ModifiedAt = userProfile.ModifiedAt
            };
            
             _logger.LogInformation("Successfully updated and mapped profile for UserId: {UserId}", userId);

            return userProfileDto;
        }

        public async Task<List<LookupDto>> SetUserInterestsAsync(string userId, SetUserInterestsCommand command)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }
            // Command validation [Required] ensures command.InterestIds is not null, but check command itself
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            _logger.LogInformation("Attempting to set interests for UserId: {UserId}", userId);

            // Ensure the UserProfile exists, create if not. 
            // Although we primarily modify UserInterests, updating the profile's 
            // ModifiedAt timestamp is logical, and it ensures the user record exists.
            var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(up => up.UserId == userId);
            if (userProfile == null)
            {
                _logger.LogInformation("No existing profile found for UserId: {UserId} while setting interests. Creating a new one.", userId);
                userProfile = new UserProfile { UserId = userId }; 
                _context.UserProfiles.Add(userProfile);
                // Need to save here to ensure the profile exists before adding interests 
                // OR rely on transaction to handle atomicity if FK constraint issues arise.
                // Let's rely on the transaction below.
            }

            // --- Validation --- 
            // Ensure all provided InterestIds are valid Guids and exist in the DB
            var distinctInterestIds = command.InterestIds.Distinct().ToList();
            List<Guid> validInterestIds = new List<Guid>();
            List<Guid> invalidInterestIds = new List<Guid>();

            if (distinctInterestIds.Any()) // Only query if there are IDs provided
            {
                 _logger.LogInformation("Validating {Count} distinct InterestIds for UserId: {UserId}", distinctInterestIds.Count, userId);
                 validInterestIds = await _context.Interests
                    .Where(i => distinctInterestIds.Contains(i.Id))
                    .Select(i => i.Id)
                    .ToListAsync();
                
                invalidInterestIds = distinctInterestIds.Except(validInterestIds).ToList();
            }
            
            if (invalidInterestIds.Any())
            {
                string invalidIdsString = string.Join(", ", invalidInterestIds);
                _logger.LogError("Validation failed: Invalid InterestIds [{InvalidIds}] provided for UserId: {UserId}", invalidIdsString, userId);
                throw new KeyNotFoundException($"The following Interest IDs were not found: {invalidIdsString}");
            }

            // --- Update Logic --- 
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Removing existing interests for UserId: {UserId}", userId);
                // Remove all existing interests for this user
                await _context.UserInterests
                    .Where(ui => ui.UserId == userId)
                    .ExecuteDeleteAsync(); 

                // Add the new interests if any were provided and validated
                if (validInterestIds.Any())
                {
                    _logger.LogInformation("Adding {Count} new interests for UserId: {UserId}", validInterestIds.Count, userId);
                    var newUserInterests = validInterestIds.Select(interestId => new UserInterest
                    {
                        UserId = userId,
                        InterestId = interestId
                    });
                    await _context.UserInterests.AddRangeAsync(newUserInterests);
                }
                else
                {
                    _logger.LogInformation("No valid interests provided or found for UserId: {UserId}. All existing interests removed.", userId);
                }

                // Update the profile's modification timestamp
                userProfile.ModifiedAt = DateTime.UtcNow;
                 _context.UserProfiles.Update(userProfile); // Ensure change tracking if profile was fetched without tracking or created

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully updated interests and profile timestamp for UserId: {UserId}", userId);
            }
            catch (Exception ex) // Catch specific DbUpdateException or general Exception
            {
                _logger.LogError(ex, "Error updating interests for UserId: {UserId}. Rolling back transaction.", userId);
                await transaction.RollbackAsync();
                throw; // Re-throw the exception to be handled globally
            }

            // --- Response Mapping ---
             _logger.LogInformation("Fetching final list of interests for response for UserId: {UserId}", userId);
             // Fetch the actual Interest entities for the response DTO
            var finalInterests = await _context.Interests
                .AsNoTracking()
                .Where(i => validInterestIds.Contains(i.Id)) // Filter by the successfully added IDs
                .Select(i => new LookupDto
                {
                    Id = i.Id,
                    Name = i.Name
                })
                .ToListAsync();

             _logger.LogInformation("Successfully mapped final interests for UserId: {UserId}", userId);
            return finalInterests;
        }
    }
} 