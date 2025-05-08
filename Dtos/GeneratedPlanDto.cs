namespace _10xVibeTravels.Dtos
{
    public class GeneratedPlanDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
    }

    public class TravelPlanAIResponse
    {
        public List<GeneratedPlanDto>? Items { get; set; }
    }
} 