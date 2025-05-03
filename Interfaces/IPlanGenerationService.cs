using _10xVibeTravels.Responses;
using _10xVibeTravels.Requests;

namespace _10xVibeTravels.Interfaces;

public interface IPlanGenerationService
{
    Task<List<PlanProposalResponse>> GeneratePlanProposalsAsync(GeneratePlanProposalRequest request, string userId);
} 