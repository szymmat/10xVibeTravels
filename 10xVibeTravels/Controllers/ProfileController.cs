using _10xVibeTravels.Interfaces;
using _10xVibeTravels.Requests;
using _10xVibeTravels.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace _10xVibeTravels.Controllers
{
    [ApiController]
    [Route("api/v1/profile")]
    [Authorize] // Requires authentication for all actions in this controller
    public class ProfileController : ControllerBase
    {
        private readonly IUserProfileService _userProfileService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IUserProfileService userProfileService, ILogger<ProfileController> logger)
        {
            _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Helper method to get UserId safely
        private string GetUserIdOrThrow()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var userId = "ab801e53-22e2-4f57-bd49-649c90da1a7d";
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("User ID not found in claims.");
                // This should theoretically not happen if [Authorize] is working correctly,
                // but best practice is to handle it.
                throw new UnauthorizedAccessException("User ID not found in claims.");
            }
            return userId;
        }

        // GET /api/v1/profile
        [HttpGet(Name = "GetUserProfile")] // Added Name for potential linking
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dtos.UserProfileDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetUserIdOrThrow();
                _logger.LogInformation("Received request to get profile for UserId: {UserId}", userId);

                var profileDto = await _userProfileService.GetUserProfileAsync(userId);

                if (profileDto == null)
                {
                    _logger.LogWarning("Profile not found for UserId: {UserId}", userId);
                    return NotFound(); // 404 Not Found as per plan
                }

                _logger.LogInformation("Successfully retrieved profile for UserId: {UserId}", userId);
                return Ok(profileDto); // 200 OK with UserProfileDto
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in GetProfile.");
                return Unauthorized(); // Should be caught by [Authorize], but belt and suspenders
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while getting the user profile.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // PATCH /api/v1/profile
        [HttpPatch(Name = "UpdateUserProfile")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Dtos.UserProfileDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command)
        {
            if (!ModelState.IsValid) // Basic model validation
            {
                _logger.LogWarning("UpdateProfile request failed model validation.");
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserIdOrThrow();
                _logger.LogInformation("Received request to update profile for UserId: {UserId}", userId);

                var updatedProfileDto = await _userProfileService.UpdateUserProfileAsync(userId, command);

                _logger.LogInformation("Successfully updated profile for UserId: {UserId}", userId);
                return Ok(updatedProfileDto); // 200 OK with updated UserProfileDto
            }
            catch (KeyNotFoundException ex) // Handle validation errors from service
            {
                _logger.LogWarning(ex, "UpdateProfile failed due to invalid ID.");
                return BadRequest(new { message = ex.Message }); // 400 Bad Request
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in UpdateProfile.");
                return Unauthorized(); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating the user profile.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while updating profile.");
            }
        }

        // PUT /api/v1/profile/interests
        [HttpPut("interests", Name = "SetUserInterests")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<LookupDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetInterests([FromBody] SetUserInterestsCommand command)
        {
            // ModelState validation handles the [Required] on command.InterestIds
            if (!ModelState.IsValid)
            {
                 _logger.LogWarning("SetInterests request failed model validation.");
                return BadRequest(ModelState);
            }

            try
            {
                var userId = GetUserIdOrThrow();
                 _logger.LogInformation("Received request to set interests for UserId: {UserId}", userId);

                var interestsDtoList = await _userProfileService.SetUserInterestsAsync(userId, command);

                _logger.LogInformation("Successfully set interests for UserId: {UserId}", userId);
                return Ok(interestsDtoList); // 200 OK with List<LookupDto>
            }
            catch (KeyNotFoundException ex) // Handle validation errors from service
            {
                _logger.LogWarning(ex, "SetInterests failed due to invalid ID.");
                return BadRequest(new { message = ex.Message }); // 400 Bad Request
            }
             catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt in SetInterests.");
                return Unauthorized(); 
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "An unexpected error occurred while setting user interests.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while setting interests.");
            }
        }
    }
} 