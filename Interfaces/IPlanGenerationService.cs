using Requests;
using Responses;

namespace _10xVibeTravels.Interfaces;

public interface IPlanGenerationService
{
    Task<List<PlanProposalResponse>> GeneratePlanProposalsAsync(GeneratePlanProposalRequest request, string userId);
} 