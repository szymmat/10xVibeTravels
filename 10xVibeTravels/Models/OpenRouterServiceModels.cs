using Newtonsoft.Json;

namespace _10xVibeTravels.Models
{
    // As per plan section 5: Zdefiniuj modele pomocnicze
    public record ModelParameters(double Temperature, [property: JsonProperty("max_Tokens")] int Max_Tokens);

    public record ResponseFormat(string Type, JsonSchema json_schema);

    public class JsonSchema
    {
        public string? Name { get; set; }
        [JsonIgnore]
        public bool Strict { get; set; }
        public object? Schema { get; set; }
    }
} 