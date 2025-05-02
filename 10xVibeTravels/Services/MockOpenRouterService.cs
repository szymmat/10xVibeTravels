namespace _10xVibeTravels.Services;

public class MockOpenRouterService : IOpenRouterService
{
    public Task<List<GeneratedPlanDto>> GeneratePlanProposalsAsync(string prompt)
    {
        // Simulate API call delay
        // await Task.Delay(1500);

        // Return mock data
        var mockProposals = new List<GeneratedPlanDto>
        {
            new GeneratedPlanDto { Title = "Mock Plan 1: City Exploration", Content = "Day 1: Visit museum, Day 2: Explore old town..." },
            new GeneratedPlanDto { Title = "Mock Plan 2: Relaxing Beach Getaway", Content = "Day 1: Beach time, Day 2: Spa day..." },
            new GeneratedPlanDto { Title = "Mock Plan 3: Adventure Trip", Content = "Day 1: Hiking, Day 2: Kayaking..." }
        };

        return Task.FromResult(mockProposals);
    }
} 