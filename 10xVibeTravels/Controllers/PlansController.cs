using _10xVibeTravels.Dtos; // For UpdatePlanDto
using _10xVibeTravels.Interfaces; // For IPlanService
using _10xVibeTravels.Requests; // For PlanListQueryParameters
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 
using System.Security.Claims; // For User.FindFirstValue

namespace _10xVibeTravels.Controllers;

[Route("api/v1/plans")]
[ApiController]
//[Authorize] // Require authentication for all actions
public class PlansController : ControllerBase
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    private string GetUserId()
    {
        // Retrieve the User ID from the current authenticated user's claims
        //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = "ab801e53-22e2-4f57-bd49-649c90da1a7d";
        if (string.IsNullOrEmpty(userId))
        {
            // This should not happen if [Authorize] is working correctly, but good practice to handle
            throw new InvalidOperationException("User ID not found in claims.");
        }
        return userId;
    }

    // GET /api/v1/plans
    [HttpGet]
    public async Task<IActionResult> GetPlans([FromQuery] PlanListQueryParameters queryParams)
    {
        // Basic validation is handled by model binding + DataAnnotations/FluentValidation
        // More complex validation (if needed) could be here or in the service
        var userId = GetUserId();
        var result = await _planService.GetPlansAsync(userId, queryParams);
        return Ok(result); // Returns 200 OK with PaginatedResult<PlanListItemDto>
    }

    // GET /api/v1/plans/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPlan(Guid id)
    {
        var userId = GetUserId();
        var plan = await _planService.GetPlanByIdAsync(userId, id);

        if (plan == null)
        {
            return NotFound(); // Returns 404 Not Found
        }

        return Ok(plan); // Returns 200 OK with PlanDetailDto
    }

    // PATCH /api/v1/plans/{id}
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdatePlanDto updateDto)
    {
        // Basic validation handled by model binding/attributes
        if (!ModelState.IsValid) // Check for basic DTO validation errors (e.g., MaxLength)
        {
            return BadRequest(ModelState);
        }
        
        // Ensure at least one field is being updated in the PATCH request
        if (updateDto.Status == null && updateDto.Title == null && updateDto.Content == null)
        {
            return BadRequest("At least one field (Status, Title, or Content) must be provided for update.");
        }

        var userId = GetUserId();
        var (updatedPlan, errorMessage) = await _planService.UpdatePlanAsync(userId, id, updateDto);

        if (errorMessage != null)
        {
            // Determine status code based on error (could be 400, 404, 403 - service should guide this, but here we check message)
            if (errorMessage.Contains("not found"))
            {
                return NotFound(new { message = errorMessage }); // 404
            }
            // Other errors are likely validation/business logic errors
            return BadRequest(new { message = errorMessage }); // 400
        }

        return Ok(updatedPlan); // Returns 200 OK with updated PlanDetailDto
    }

    // DELETE /api/v1/plans/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePlan(Guid id)
    {
        var userId = GetUserId();
        await _planService.DeletePlanAsync(userId, id);

        // Service returns success even if not found (idempotency), so no error check needed here normally.
        // If we wanted to return 404 specifically if it wasn't found *before* delete, service logic would need adjustment.
        
        // Based on plan, we expect 204 No Content on success
        return NoContent(); 
    }
} 