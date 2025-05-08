# OpenRouter Service Implementation Plan

## 1. Opis usługi
Usługa `OpenRouterService` zapewnia abstrakcję nad API OpenRouter (usługa Openrouter.ai), umożliwiającą wysyłanie wieloczęściowych czatów z wiadomościami systemowymi i użytkownika do dowolnego LLM oraz odbiór ustrukturyzowanych odpowiedzi w formacie JSON.

Kluczowe cele:
- Upraszczanie budowy i wysyłki zapytań do OpenRouter API
- Centralizacja konfiguracji modelu (nazwa, parametry)
- Weryfikacja i deserializacja odpowiedzi zgodnie ze ściśle zdefiniowanym `response_format`
- Obsługa błędów i retry logic

## 2. Opis konstruktora
```csharp
public OpenRouterService(IOptions<OpenRouterSettings> settings, HttpClient httpClient, ILogger<OpenRouterService> logger)
```
Parametry:
- `settings` – zawiera `ApiKey`, `BaseUrl`, domyślny `ModelName`, domyślne `ModelParameters`.
- `httpClient` – skonfigurowany w DI z podstawowym `BaseUrl` i nagłówkiem `Authorization: Bearer {ApiKey}`.
- `logger` – do logowania wywołań i błędów.

## 3. Publiczne metody i pola

### 3.1 SendChatAsync
```csharp
Task<TResponse> SendChatAsync<TResponse>(
    string systemMessage,
    string userMessage,
    string? modelName = null,
    ModelParameters? parameters = null,
    ResponseFormat? responseFormat = null
)
```
- `systemMessage` – komunikat systemowy 1.
- `userMessage` – komunikat użytkownika 2.
- `modelName` – nazwa modelu (np. `gpt-4`).
- `parameters` – parametry modelu (np. `temperature`, `max_tokens`).
- `responseFormat` – definicja ustrukturyzowanej odpowiedzi 3.

Zwraca: deserializowaną odpowiedź typu `TResponse`.

## 4. Prywatne metody i pola

### 4.1 BuildRequestBody
Tworzy obiekt żądania:
```csharp
var payload = new {
  model = modelName,
  messages = new [] {
    new { role = "system", content = systemMessage },
    new { role = "user", content = userMessage }
  },
  model_params = parameters,
  response_format = responseFormat
};
```

### 4.2 HandleResponseAsync
- Deserializuje odpowiedź JSON.
- Waliduje zgodność z `json_schema` (jeśli podano `responseFormat`).
- Rzuca wyjątek `SchemaViolationException`, jeśli walidacja się nie powiedzie.

### 4.3 RetryPolicy
- Prosta strategia retry przy kodach 5xx i 429.

## 5. Obsługa błędów
1. HTTP 4xx (np. 401, 403): nieautoryzowany lub brak uprawnień.
2. HTTP 429: limit rate, retry po `Retry-After`.
3. HTTP 5xx: błąd po stronie serwera, retry z backoff.
4. JSON parse error: nieprawidłowa odpowiedź.
5. SchemaViolationException: odpowiedź niezgodna ze schematem.

## 6. Kwestie bezpieczeństwa
- Przechowywanie klucza API w Azure Key Vault / appsettings.UserSecrets.
- Ustawienie `HttpClient` z `HttpClientFactory` i TLS 1.2+
- Ograniczenie wielkości wiadomości i parametrów (walidacja wejścia).
- Logowanie bez wycieku `ApiKey` lub treści prywatnych.

## 7. Plan wdrożenia krok po kroku

1. Dodaj pakiety NuGet:
   - `Microsoft.Extensions.Http`
   - `Microsoft.Extensions.Options`
   - `Flurl.Http` (opcjonalnie)
   - `Newtonsoft.Json` / `System.Text.Json`
2. Skonfiguruj `OpenRouterSettings` w `appsettings.json`:
   ```json
   "OpenRouterSettings": {
     "ApiKey": "<TWÓJ_API_KEY>",
     "BaseUrl": "https://openrouter.ai/v1/chat/completions",
     "ModelName": "gpt-4o-mini",
     "ModelParameters": { "temperature": 0.7, "max_tokens": 512 }
   }
   ```
3. Zarejestruj `HttpClient` i serwis w `Startup.cs` / `Program.cs`:
   ```csharp
   builder.Services.Configure<OpenRouterSettings>(configuration.GetSection("OpenRouterSettings"));
   builder.Services.AddHttpClient<OpenRouterService>((sp, client) => {
     var settings = sp.GetRequiredService<IOptions<OpenRouterSettings>>().Value;
     client.BaseAddress = new Uri(settings.BaseUrl);
     client.DefaultRequestHeaders.Authorization = new("Bearer", settings.ApiKey);
   });
   builder.Services.AddScoped<OpenRouterService>();
   ```
4. Zaimplementuj klasę `OpenRouterService` zgodnie z opisem: 
   - Konstruktor
   - `SendChatAsync`
   - Metody pomocnicze `BuildRequestBody`, `HandleResponseAsync`
5. Zdefiniuj modele pomocnicze:
   ```csharp
   public record ModelParameters(double temperature, int max_tokens);

   public record ResponseFormat(string type, JsonSchema json_schema);

   public class JsonSchema {
     public string name { get; set; }
     public bool strict { get; set; }
     public object schema { get; set; }
   }
   ```
6. Dodaj obsługę błędów i retry w `SendChatAsync`.
7. Przetestuj lokalnie, wysyłając przykładowe żądanie:
   ```csharp
   var response = await openRouterService.SendChatAsync<ChatReply>(
     systemMessage: "You are a helpful assistant.",
     userMessage: "What is the capital of France?",
     responseFormat: new ResponseFormat(
       type: "json_schema",
       json_schema: new JsonSchema {
         name = "capitalResponse",
         strict = true,
         schema = new { capital = "string" }
       }
     )
   );
   ```

---

W powyższym planie uwzględniono wszystkie kluczowe komponenty, obsługę błędów, kwestie bezpieczeństwa oraz przykłady konfiguracji `systemMessage`, `userMessage`, `response_format`, `modelName` i parametrów modelu. 