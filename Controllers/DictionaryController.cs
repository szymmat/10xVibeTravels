using _10xVibeTravels.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace _10xVibeTravels.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // Base route, though specific routes are on actions
    public class DictionaryController : ControllerBase
    {
        private readonly IInterestService _interestService;
        private readonly ITravelStyleService _travelStyleService;
        private readonly IIntensityService _intensityService;
        private readonly ILogger<PlanProposalsController> _logger;

        public DictionaryController(
            IInterestService interestService, 
            ITravelStyleService travelStyleService, 
            IIntensityService intensityService, ILogger<PlanProposalsController> logger)
        {
            _interestService = interestService;
            _travelStyleService = travelStyleService;
            _intensityService = intensityService;
            _logger = logger;
        }

        [HttpGet("interests")]
        [ProducesResponseType(typeof(IEnumerable<Dtos.InterestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInterests()
        {
            try
            {
                var interests = await _interestService.GetAllAsync();
                return Ok(interests);
            }
            catch (Exception ex) // Basic exception handling
            {
                _logger.LogError(ex, "Error retrieving interests"); 
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving interests.");
            }
        }

        [HttpGet("travel-styles")]
        [ProducesResponseType(typeof(IEnumerable<Dtos.TravelStyleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTravelStyles()
        {
            try
            {
                var travelStyles = await _travelStyleService.GetAllAsync();
                return Ok(travelStyles);
            }
            catch (Exception ex) // Basic exception handling
            {
                _logger.LogError(ex, "Error retrieving travel styles");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving travel styles.");
            }
        }

        [HttpGet("intensities")]
        [ProducesResponseType(typeof(IEnumerable<Dtos.IntensityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetIntensities()
        {
            try
            {
                var intensities = await _intensityService.GetAllAsync();
                return Ok(intensities);
            }
            catch (Exception ex) // Basic exception handling
            {
                _logger.LogError(ex, "Error retrieving intensities");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving intensities.");
            }
        }

        // Actions will be added here
    }
} 