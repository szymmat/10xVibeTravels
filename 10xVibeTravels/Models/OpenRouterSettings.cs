namespace _10xVibeTravels.Models
{
    public class OpenRouterSettings
    {
        public string? ApiKey { get; set; }
        public string? BaseUrl { get; set; }
        public string? ModelName { get; set; }
        public ModelParameters? DefaultModelParameters { get; set; } // Reusing ModelParameters from OpenRouterServiceModels
    }
} 