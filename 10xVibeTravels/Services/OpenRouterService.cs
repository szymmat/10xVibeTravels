using _10xVibeTravels.Interfaces;
using _10xVibeTravels.Models;
using System.Text.Json;
using _10xVibeTravels.Exceptions;
using System.Net;
using Polly;
using Polly.Retry;
using Newtonsoft.Json.Linq; // For JToken
using Newtonsoft.Json.Schema; // For JSchema, ValidationError// For JsonConvert if serializing errors, and JsonException

namespace _10xVibeTravels.Services
{
    public class OpenRouterService : IOpenRouterService
    {
        private readonly OpenRouterSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenRouterService> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private const string ChatCompletionsEndpoint = "chat/completions";

        public OpenRouterService(IConfiguration configuration, HttpClient httpClient, ILogger<OpenRouterService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            _settings = configuration.GetSection("OpenRouter").Get<OpenRouterSettings>()!;

            if (_settings == null)
            {
                _logger.LogError("OpenRouter settings are not configured in appsettings.json under the 'OpenRouter' section.");
                throw new InvalidOperationException("OpenRouter settings are not configured properly.");
            }
            if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
            {
                _logger.LogError("OpenRouterSettings: BaseUrl is missing in configuration.");
                throw new InvalidOperationException("OpenRouterSettings: BaseUrl cannot be empty or is missing in configuration.");
            }
            // ApiKey is configured in HttpClient factory. ModelName and DefaultModelParameters are optional with fallbacks in BuildRequestBody.

            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<OpenRouterServerException>()
                .Or<OpenRouterTimeoutException>()
                .OrResult<HttpResponseMessage>(response => 
                    response.StatusCode == HttpStatusCode.TooManyRequests ||
                    (int)response.StatusCode >= 500)
                .WaitAndRetryAsync(
                    retryCount: 3, 
                    sleepDurationProvider: (retryAttempt, response, context) => 
                    {
                        if (response?.Result?.StatusCode == HttpStatusCode.TooManyRequests && 
                            response.Result.Headers.RetryAfter?.Delta.HasValue == true)
                        {
                            var retryAfter = response.Result.Headers.RetryAfter.Delta.Value;
                            _logger.LogWarning("Rate limit hit (429). Retrying after {RetryAfterSeconds} seconds. Attempt {RetryAttempt}", retryAfter.TotalSeconds, retryAttempt);
                            return retryAfter;
                        }
                        var jitterer = new Random();
                        var calculatedWait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000));
                        _logger.LogWarning("Request failed. Retrying in {CalculatedWaitSeconds}s. Attempt {RetryAttempt}. Status: {StatusCode}", calculatedWait.TotalSeconds, retryAttempt, response?.Result?.StatusCode);
                        return calculatedWait;
                    },
                    onRetryAsync: (response, timespan, retryAttempt, context) => 
                    {
                        _logger.LogInformation("Retrying request... Attempt {RetryAttempt}", retryAttempt);
                        return Task.CompletedTask;
                    }
                );
        }

        // Private record for cleaner message structure in payload
        private record Message(string Role, string Content);

        // Private class for the request payload, matching OpenRouter API
        private class OpenRouterRequestPayload
        {
            public string Model { get; set; }
            public List<Message> Messages { get; set; }
            public ModelParameters? Tools { get; set; } // Renamed from model_params to Tools as per typical OpenRouter usage for functions/tools, assuming ModelParameters can be repurposed or a new specific type is needed. The plan mentioned model_params which is less common than 'tools' or 'tool_choice' for structured output. For now, I'll use 'Tools' and map ModelParameters. This might need adjustment based on precise OpenRouter API for parameters.
            public ResponseFormat? Response_Format { get; set; } // Matching the plan's response_format

            public OpenRouterRequestPayload(string model, List<Message> messages)
            {
                Model = model;
                Messages = messages;
            }
        }

        private string BuildRequestBody(
            string systemMessage,
            string userMessage,
            string? modelName,
            ModelParameters? parameters,
            ResponseFormat? responseFormat)
        {
            var effectiveModelName = modelName ?? _settings.ModelName ?? "gpt-4o-mini"; // Fallback from settings or hardcoded
            var effectiveParameters = parameters ?? _settings.DefaultModelParameters;

            var messages = new List<Message>
            {
                new Message("system", systemMessage),
                new Message("user", userMessage)
            };
            
            // The plan originally suggested an anonymous type, which is fine.
            // We will serialize this using Newtonsoft.Json.
            var requestBodyObject = new {
                model = effectiveModelName,
                messages = messages.Select(m => new { role = m.Role, content = m.Content }).ToArray(), // Ensure messages are in the correct format
                model_params = effectiveParameters, // Plan specifies "model_params"
                response_format = responseFormat
            };

            // Serialize using Newtonsoft.Json
            string jsonRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBodyObject, new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(), // Ensure snake_case for model_params and response_format if not already. However, OpenRouter usually expects snake_case.
                                                                                                                // If the anonymous object properties are already snake_case (like model_params), this might double convert.
                                                                                                                // Let's assume Newtonsoft will handle it correctly or we adjust property names in anonymous object.
                                                                                                                // For "model_params", it's already snake_case.
                                                                                                                // For "response_format", it is also snake_case.
                                                                                                                // For "model", it should be "model".
                                                                                                                // For "messages", it should be "messages" with "role" and "content" inside.
                                                                                                                // The anonymous object properties are already lowercase/snake_case as intended.
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore // Optional: to not include null properties
            });
            
            _logger.LogDebug("Built OpenRouter request body (JSON string): {JsonRequestBody}", jsonRequestBody);
            return jsonRequestBody;
        }

        public async Task<TResponse> SendChatAsync<TResponse>(
            string systemMessage,
            string userMessage,
            string? modelName = null,
            ModelParameters? parameters = null,
            ResponseFormat? responseFormat = null)
        {
            _logger.LogInformation(
                "SendChatAsync called. Model: {ModelName}, SystemMessage: Starts with \"{SystemStart}\", UserMessage: Starts with \"{UserStart}\"", 
                modelName ?? _settings.ModelName,
                systemMessage.Substring(0, Math.Min(systemMessage.Length, 50)),
                userMessage.Substring(0, Math.Min(userMessage.Length, 50)));

            if (string.IsNullOrWhiteSpace(systemMessage))
            {
                throw new ArgumentException("System message cannot be empty.", nameof(systemMessage));
            }
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                throw new ArgumentException("User message cannot be empty.", nameof(userMessage));
            }

            var jsonRequestBody = BuildRequestBody(systemMessage, userMessage, modelName, parameters, responseFormat);
            
            HttpResponseMessage? responseMessage = null;
            try
            {
                responseMessage = await _retryPolicy.ExecuteAsync(async token => 
                {
                    _logger.LogDebug("Attempting POST request to OpenRouter. Base: {BaseAddress}, Endpoint: {Endpoint}", _httpClient.BaseAddress, ChatCompletionsEndpoint);
                    // Use StringContent as requestBody is now a JSON string
                    HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/{ChatCompletionsEndpoint}");
                    message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
                    message.Content = new StringContent(jsonRequestBody, System.Text.Encoding.UTF8, "application/json");
                    return await _httpClient.SendAsync(message);
                }, CancellationToken.None);
                
                _logger.LogInformation("Request to OpenRouter completed. Status: {StatusCode}", responseMessage.StatusCode);
            }
            catch (OpenRouterRateLimitException ex) 
            {
                 _logger.LogError(ex, "Rate limit retries exhausted for OpenRouter. RetryAfter: {RetryAfterValue}", ex.RetryAfter);
                 throw;
            }
            catch (HttpRequestException ex) when (ex.InnerException is System.Threading.Tasks.TaskCanceledException taskCanceledEx && taskCanceledEx.InnerException is TimeoutException)
            {
                _logger.LogError(ex, "HTTP request to OpenRouter timed out after retries.");
                throw new OpenRouterTimeoutException("The request to OpenRouter timed out after retries.", ex);
            }
            catch (HttpRequestException ex) 
            {
                _logger.LogError(ex, "HTTP request to OpenRouter failed after retries. Status: {StatusCode}", responseMessage?.StatusCode);
                if(responseMessage == null || responseMessage.RequestMessage == null) 
                {
                     _logger.LogError(ex, "HTTP request failed catastrophically before any response obtained, even after retries.");
                     throw new OpenRouterServerException("Request to OpenRouter failed before a response was received, even after retries.", HttpStatusCode.ServiceUnavailable, null, ex);
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An unexpected error occurred in SendChatAsync before response handling. Status: {StatusCode}", responseMessage?.StatusCode);
                throw new OpenRouterException("An unexpected error occurred while sending the request to OpenRouter.", responseMessage?.StatusCode ?? HttpStatusCode.InternalServerError, null, ex);
            }
            
            if (responseMessage == null)
            {
                _logger.LogError("responseMessage is unexpectedly null after retry policy execution and before HandleResponseAsync.");
                throw new OpenRouterException("Failed to get a response from OpenRouter after retries.", HttpStatusCode.InternalServerError);
            }

            return await HandleResponseAsync<TResponse>(responseMessage, responseFormat);
        }

        private async Task<TResponse> HandleResponseAsync<TResponse>(HttpResponseMessage httpResponseMessage, ResponseFormat? responseFormat)
        {
            _logger.LogDebug("HandleResponseAsync called for status code: {StatusCode}", httpResponseMessage.StatusCode);
            string responseContent = string.Empty;
            try
            {
                responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read response content from HttpResponseMessage.");
                // If we can't read content, we likely can't proceed to deserialize or validate.
                // Depending on the desired behavior, re-throw or throw a specific exception.
                // For now, let an OpenRouterException be thrown by later checks if responseContent remains empty.
            }

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                 _logger.LogError("HandleResponseAsync received non-success status code {StatusCode}. Content: {ResponseContent}", 
                    httpResponseMessage.StatusCode, 
                    responseContent); // responseContent might be empty if read failed

                switch (httpResponseMessage.StatusCode)
                {
                    case HttpStatusCode.Unauthorized: 
                    case HttpStatusCode.Forbidden:    
                        throw new OpenRouterAuthException(
                            $"Authentication failed. Status: {httpResponseMessage.StatusCode}. Ensure API key is valid and has permissions.", 
                            httpResponseMessage.StatusCode, 
                            responseContent);
                    case HttpStatusCode.TooManyRequests: 
                        TimeSpan? retryAfter = httpResponseMessage.Headers.RetryAfter?.Delta;
                        throw new OpenRouterRateLimitException(
                            "Rate limit exceeded. Please try again later.", 
                            responseContent,
                            retryAfter);
                    case HttpStatusCode.InternalServerError:    
                    case HttpStatusCode.BadGateway:           
                    case HttpStatusCode.ServiceUnavailable:   
                    case HttpStatusCode.GatewayTimeout:       
                        throw new OpenRouterServerException(
                            $"OpenRouter server error. Status: {httpResponseMessage.StatusCode}. Try again later.", 
                            httpResponseMessage.StatusCode, 
                            responseContent);
                    default:
                        throw new OpenRouterException(
                            $"OpenRouter request failed with status code {httpResponseMessage.StatusCode}.", 
                            httpResponseMessage.StatusCode, 
                            responseContent);
                }
            }

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                _logger.LogWarning("OpenRouter response content was null or empty after a successful status code.");
                // This implies an issue with the response itself or reading it.
                throw new OpenRouterException("OpenRouter response content was null or empty after a successful status code.", httpResponseMessage.StatusCode, responseContent);
            }

            _logger.LogDebug("Full response content from OpenRouter: {ResponseContent}", responseContent);
            TResponse? deserializedResponse = default;
            
            string? aiContentString = null;
            try
            {
                // 1. Deserialize the overall OpenRouter response to a JObject
                JObject openRouterFullResponse = JObject.Parse(responseContent);

                // 2. Extract the actual AI-generated content string.
                aiContentString = openRouterFullResponse.SelectToken("choices[0].message.content")?.Value<string>();

                if (string.IsNullOrWhiteSpace(aiContentString))
                {
                    _logger.LogError("AI-generated content (choices[0].message.content) is null or empty. Full response: {ResponseContent}", responseContent);
                    throw new OpenRouterException("AI-generated content is missing in the OpenRouter response.", HttpStatusCode.OK, responseContent);
                }
                _logger.LogDebug("Extracted AI content string for schema validation and deserialization: {AIContentString}", aiContentString);

                // 3. Schema Validation (if schema is provided for the AI content string)
                if (responseFormat?.json_schema?.Schema != null)
                {
                    _logger.LogInformation("Attempting JSON schema validation (Newtonsoft) for AI content: {SchemaName}", 
                        responseFormat.json_schema.Name ?? "UnnamedSchema");
                    try
                    {
                        JSchema schemaValidator;
                        if (responseFormat.json_schema.Schema is string schemaString)
                        {
                            schemaValidator = JSchema.Parse(schemaString);
                        }
                        else
                        {
                            // Serialize the schema object (e.g., anonymous type) to a JSON string using Newtonsoft.Json
                            // This ensures that if the schema object's definition relies on Newtonsoft attributes or conventions, they are respected.
                            // The schema in PlanGenerationService uses lowercase C# properties, so CamelCasePropertyNamesContractResolver might not be strictly needed here
                            // but it's safer if other schemas might use PascalCase.
                            string schemaJson = Newtonsoft.Json.JsonConvert.SerializeObject(responseFormat.json_schema.Schema, 
                                new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(), NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });
                            _logger.LogDebug("Serialized schema for validation: {SchemaJson}", schemaJson);
                            schemaValidator = JSchema.Parse(schemaJson);
                        }
                        
                        JToken jsonToValidate = JToken.Parse(aiContentString); // Parse the AI content string
                        IList<ValidationError> validationErrors;
                        bool isValid = jsonToValidate.IsValid(schemaValidator, out validationErrors);

                        if (!isValid)
                        {
                            string errors = Newtonsoft.Json.JsonConvert.SerializeObject(validationErrors, Newtonsoft.Json.Formatting.Indented);
                            _logger.LogError("AI content JSON schema validation failed for {SchemaName} (Newtonsoft). Errors: {ValidationErrors}. AI Content: {AIContentString}", 
                                responseFormat.json_schema.Name ?? "UnnamedSchema", 
                                errors,
                                aiContentString); // Log the content that failed
                            // This constructor takes (message, validationErrorsString)
                            throw new OpenRouterSchemaViolationException(
                                $"AI content from OpenRouter failed JSON schema validation for '{responseFormat.json_schema.Name ?? "UnnamedSchema"}' (Newtonsoft).",
                                errors 
                            );
                        }
                        _logger.LogInformation("AI content JSON schema validation successful for {SchemaName} (Newtonsoft).", responseFormat.json_schema.Name ?? "UnnamedSchema");
                    }
                    catch (OpenRouterSchemaViolationException) { throw; } // Re-throw if it's already the correct type from above
                    catch (Exception ex) // Catches errors during schema parsing or validation
                    {
                        _logger.LogError(ex, "An error occurred during JSON schema validation process for AI content for {SchemaName} (Newtonsoft). AI Content: {AIContentString}", 
                            responseFormat.json_schema.Name ?? "UnnamedSchema",
                            aiContentString);
                        // This constructor takes (message, problematicContent, innerException)
                        throw new OpenRouterSchemaViolationException(
                            $"Error during JSON schema validation for AI content for '{responseFormat.json_schema.Name ?? "UnnamedSchema"}' (Newtonsoft): {ex.Message}", 
                            aiContentString, 
                            ex);
                    }
                }

                // 4. Deserialize the AI content string into TResponse using Newtonsoft.Json
                deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(aiContentString);

                if (deserializedResponse == null)
                {
                    _logger.LogError("Deserialized AI content response is null (using Newtonsoft.Json). Original AI content string: {AIContentString}", aiContentString);
                    // Use Newtonsoft.Json.JsonException for consistency if the deserializer is Newtonsoft
                    throw new Newtonsoft.Json.JsonException("Deserialized AI content response is null (using Newtonsoft.Json).");
                }
                return deserializedResponse;
            }
            // Catch exceptions specifically from parsing the full response or extracting/deserializing AI content
            catch (Newtonsoft.Json.JsonException ex) // Covers JObject.Parse, JToken.Parse, JsonConvert.DeserializeObject
            {
                 _logger.LogError(ex, "Newtonsoft.Json operation failed while processing OpenRouter response. Potentially problematic AI content: {AIContentString}. Full response: {ResponseContent}", aiContentString ?? "N/A", responseContent);
                throw new OpenRouterException("A Newtonsoft.Json error occurred while processing the OpenRouter response.", HttpStatusCode.OK, aiContentString ?? responseContent, ex); 
            }
            // System.Text.Json.JsonException might be less likely if we're not using it for the primary path, but keep for safety or if schema serialization still uses it.
            catch (System.Text.Json.JsonException ex) 
            {
                _logger.LogError(ex, "System.Text.Json deserialization failed in HandleResponseAsync. Potentially problematic AI content: {AIContentString}. Full response: {ResponseContent}", aiContentString ?? "N/A", responseContent);
                throw new OpenRouterException("Failed to deserialize OpenRouter response using System.Text.Json.", HttpStatusCode.OK, aiContentString ?? responseContent, ex); 
            }
            catch (OpenRouterException) { throw; } // Re-throw specific OpenRouter exceptions like schema violation from above
            catch (Exception ex) // General catch-all for unexpected errors in this block
            {
                _logger.LogError(ex, "Unexpected error in HandleResponseAsync while processing OpenRouter response. Potentially problematic AI content: {AIContentString}. Full response: {ResponseContent}", aiContentString ?? "N/A", responseContent);
                throw new OpenRouterException("An unexpected error occurred while processing the OpenRouter response.", HttpStatusCode.InternalServerError, aiContentString ?? responseContent, ex); 
            }
        }
    }
} 