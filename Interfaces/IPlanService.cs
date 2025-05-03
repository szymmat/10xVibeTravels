using _10xVibeTravels.Dtos;
using _10xVibeTravels.Requests;
using _10xVibeTravels.Responses;

namespace _10xVibeTravels.Interfaces;

public interface IPlanService
{
    Task<PaginatedResult<PlanListItemDto>> GetPlansAsync(string userId, PlanListQueryParameters queryParams);
    Task<PlanDetailDto?> GetPlanByIdAsync(string userId, Guid planId);
    Task<(PlanDetailDto? UpdatedPlan, string? ErrorMessage)> UpdatePlanAsync(string userId, Guid planId, UpdatePlanDto updateDto);
    Task<(bool Success, string? ErrorMessage)> DeletePlanAsync(string userId, Guid planId);
} 