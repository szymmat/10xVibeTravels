namespace _10xVibeTravels.Services;

public interface IOpenRouterService
{
    Task<List<GeneratedPlanDto>> GeneratePlanProposalsAsync(string prompt);
}
