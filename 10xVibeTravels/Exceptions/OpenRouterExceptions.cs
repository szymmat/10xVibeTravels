using System;
using System.Net;

namespace _10xVibeTravels.Exceptions
{
    public class OpenRouterException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string? ErrorContent { get; }

        public OpenRouterException(string message, HttpStatusCode statusCode, string? errorContent = null, Exception? innerException = null)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            ErrorContent = errorContent;
        }
    }

    public class OpenRouterAuthException : OpenRouterException
    {
        public OpenRouterAuthException(string message, HttpStatusCode statusCode, string? errorContent = null, Exception? innerException = null)
            : base(message, statusCode, errorContent, innerException)
        { }
    }

    public class OpenRouterRateLimitException : OpenRouterException
    {
        public TimeSpan? RetryAfter { get; }
        public OpenRouterRateLimitException(string message, string? errorContent = null, TimeSpan? retryAfter = null, Exception? innerException = null)
            : base(message, HttpStatusCode.TooManyRequests, errorContent, innerException)
        {
            RetryAfter = retryAfter;
        }
    }

    public class OpenRouterServerException : OpenRouterException
    {
        public OpenRouterServerException(string message, HttpStatusCode statusCode, string? errorContent = null, Exception? innerException = null)
            : base(message, statusCode, errorContent, innerException)
        { }
    }

    public class OpenRouterSchemaViolationException : OpenRouterException
    {
        public OpenRouterSchemaViolationException(string message, string? errorContent = null, Exception? innerException = null)
            : base(message, HttpStatusCode.BadRequest, errorContent, innerException) // Or another appropriate status
        { }
    }
    
    public class OpenRouterTimeoutException : OpenRouterException
    {
        public OpenRouterTimeoutException(string message, Exception? innerException = null)
            : base(message, HttpStatusCode.RequestTimeout, null, innerException)
        { }
    }
} 