using _10xVibeTravels.Features.PlanProposals.Requests;
using _10xVibeTravels.Features.PlanProposals.Responses;

namespace _10xVibeTravels.Features.PlanProposals.Services;

public interface IPlanGenerationService
{
    Task<List<PlanProposalResponse>> GeneratePlanProposalsAsync(GeneratePlanProposalRequest request, string userId);
} 