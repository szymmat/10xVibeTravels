using _10xVibeTravels.Dtos;
using _10xVibeTravels.Requests;

namespace _10xVibeTravels.Interfaces
{
    public interface IUserProfileService
    {
        /// <summary>
        /// Gets the user profile for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user profile DTO, or null if not found.</returns>
        Task<UserProfileDto?> GetUserProfileAsync(string userId);

        /// <summary>
        /// Updates the user profile for the specified user.
        /// Creates the profile if it doesn't exist.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="command">The command containing the update data.</param>
        /// <returns>The updated user profile DTO.</returns>
        Task<UserProfileDto> UpdateUserProfileAsync(string userId, UpdateUserProfileCommand command);

        /// <summary>
        /// Sets the interests for the specified user, replacing any existing interests.
        /// Creates the profile if it doesn't exist.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="command">The command containing the list of interest IDs.</param>
        /// <returns>A list of lookup DTOs representing the user's new interests.</returns>
        Task<List<LookupDto>> SetUserInterestsAsync(string userId, SetUserInterestsCommand command);
    }
} 