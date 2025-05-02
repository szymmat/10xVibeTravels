using _10xVibeTravels.Exceptions;
using _10xVibeTravels.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Requests;
using System.Security.Claims;

namespace _10xVibeTravels.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Requires authentication for all actions in this controller
public class PlanProposalsController : ControllerBase
{
    private readonly IPlanGenerationService _planGenerationService;
    private readonly ILogger<PlanProposalsController> _logger;

    public PlanProposalsController(IPlanGenerationService planGenerationService, ILogger<PlanProposalsController> logger)
    {
        _planGenerationService = planGenerationService;
        _logger = logger;
    }

    /// <summary>
    /// Generates three new travel plan proposals based on a source note and user preferences.
    /// </summary>
    /// <param name="request">Contains the source note ID, travel dates, and optional budget.</param>
    /// <returns>A list of the three generated plan proposals.</returns>
    /// <response code="201">Returns the newly created plan proposals.</response>
    /// <response code="400">If the request is invalid (e.g., missing required fields, invalid dates, negative budget).</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not authorized to use the specified note.</response>
    /// <response code="404">If the specified note is not found.</response>
    /// <response code="422">If the budget cannot be determined (not in request or profile).</response>
    /// <response code="500">If an internal server error occurs during processing or database saving.</response>
    /// <response code="503">If the external AI service is unavailable or returns an error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(List<Responses.PlanProposalResponse>), 201)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GeneratePlanProposals([FromBody] GeneratePlanProposalRequest request)
    {
        // Get user ID from the claims principal
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //var userId = "ab801e53-22e2-4f57-bd49-649c90da1a7d";

        if (string.IsNullOrEmpty(userId))
        {
            // This should ideally not happen due to [Authorize] attribute
            _logger.LogWarning("User ID not found in claims principal despite [Authorize] attribute.");
            return Unauthorized("User ID could not be determined.");
        }

        try
        {
            var result = await _planGenerationService.GeneratePlanProposalsAsync(request, userId);
            
            // Return 201 Created with the list of proposals in the body.
            // A more RESTful approach might return a Location header pointing to a GET endpoint 
            // for these plans, but returning the result directly is simpler for now and aligns with the plan.
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (NoteNotFoundException ex)
        {
            _logger.LogInformation(ex, "Plan proposal generation failed: Note not found.");
            return NotFound(new { message = ex.Message });
        }
        catch (NoteForbiddenException ex)
        {
            _logger.LogWarning(ex, "Plan proposal generation forbidden: User does not own note.");
            return Forbid(); // 403
        }
        catch (InvalidDateRangeException ex)
        {
            _logger.LogInformation(ex, "Plan proposal generation failed: Invalid date range.");
            return BadRequest(new { message = ex.Message }); // 400
        }
        catch (BudgetNotAvailableException ex)
        {
            _logger.LogInformation(ex, "Plan proposal generation failed: Budget not available.");
            return UnprocessableEntity(new { message = ex.Message }); // 422
        }
        catch (AiServiceException ex)
        {
            _logger.LogError(ex, "Plan proposal generation failed: AI service error.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
        catch (ApplicationException ex) // Catch specific application errors (like DB save failure)
        {
             _logger.LogError(ex, "Plan proposal generation failed: Application error.");
             return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected application error occurred." });
        }
        catch (Exception ex) // Catch-all for unexpected errors
        {
            _logger.LogError(ex, "Plan proposal generation failed: An unexpected error occurred.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected internal error occurred." });
        }
    }
} 