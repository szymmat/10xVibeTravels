using _10xVibeTravels.Models;

namespace _10xVibeTravels.Interfaces
{
    public interface IOpenRouterService
    {
        Task<TResponse> SendChatAsync<TResponse>(
            string systemMessage,
            string userMessage,
            string? modelName = null,
            ModelParameters? parameters = null,
            ResponseFormat? responseFormat = null
        );
    }
} 