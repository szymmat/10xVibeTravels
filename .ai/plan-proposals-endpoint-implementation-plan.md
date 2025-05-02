# API Endpoint Implementation Plan: Generate Plan Proposals

## 1. Przegląd punktu końcowego
Ten punkt końcowy (`POST /plan-proposals`) umożliwia uwierzytelnionym użytkownikom generowanie trzech propozycji planów podróży przy użyciu zewnętrznego serwisu AI (Openrouter.ai). Generowanie opiera się na istniejącej notatce użytkownika, jego preferencjach profilowych (styl podróży, intensywność, zainteresowania, domyślny budżet) oraz podanych w żądaniu datach podróży i opcjonalnym budżecie. Wygenerowane propozycje są zapisywane w bazie danych ze statusem 'Generated' i zwracane użytkownikowi.

## 2. Szczegóły żądania
- **Metoda HTTP:** `POST`
- **Struktura URL:** `/plan-proposals`
- **Parametry:** Brak parametrów query/URL.
- **Request Body:** `application/json`
  ```json
  {
    "noteId": "guid", // Wymagany - ID notatki źródłowej
    "startDate": "YYYY-MM-DD", // Wymagany - Data rozpoczęcia planu
    "endDate": "YYYY-MM-DD", // Wymagany - Data zakończenia planu (musi być późniejsza niż startDate)
    "budget": 1200.50 // Opcjonalny - Budżet planu (nadpisuje budżet z profilu, musi być >= 0)
  }
  ```

## 3. Wykorzystywane typy
- **Request Model/Command:** `GeneratePlanProposalRequest`
  ```csharp
  public class GeneratePlanProposalRequest
  {
      [Required]
      public Guid NoteId { get; set; }

      [Required]
      public DateOnly StartDate { get; set; } // Lub DateTime, jeśli preferowane

      [Required]
      public DateOnly EndDate { get; set; } // Lub DateTime

      [Range(0, (double)decimal.MaxValue)]
      public decimal? Budget { get; set; }
  }
  ```
- **Response DTO:** `PlanProposalResponse` (odzwierciedlający strukturę zdefiniowaną w specyfikacji API, prawdopodobnie mapowany z encji `Plan`)
  ```csharp
   public class PlanProposalResponse
   {
       public Guid Id { get; set; }
       public string Status { get; set; } = null!;
       public DateOnly StartDate { get; set; } // Lub DateTime
       public DateOnly EndDate { get; set; } // Lub DateTime
       public decimal Budget { get; set; }
       public string Title { get; set; } = null!;
       public string Content { get; set; } = null!;
       public DateTime CreatedAt { get; set; }
       public DateTime ModifiedAt { get; set; }
   }
  ```
- **Entity:** `Plan` (z `10xVibeTravels.Data`)

## 4. Przepływ danych
1.  Żądanie `POST /plan-proposals` trafia do kontrolera API.
2.  ASP.NET Core dokonuje walidacji modelu żądania (`GeneratePlanProposalRequest`) na podstawie atrybutów.
3.  Kontroler pobiera ID zalogowanego użytkownika (np. z `HttpContext.User` lub `UserManager`).
4.  Kontroler wywołuje metodę w serwisie `PlanGenerationService`, przekazując dane żądania i ID użytkownika.
5.  `PlanGenerationService`:
    a.  Pobiera encję `Note` o podanym `noteId` oraz `UserProfile` użytkownika (wraz z powiązanymi `Interests`, `TravelStyle`, `Intensity`) z `ApplicationDbContext`.
    b.  Sprawdza, czy notatka istnieje i czy należy do zalogowanego użytkownika.
    c.  Ustala ostateczny budżet (`budget`) na podstawie żądania lub profilu użytkownika. Weryfikuje, czy budżet jest dostępny.
    d.  Konstruuje prompt dla modelu AI, zawierający szczegóły z notatki, profilu użytkownika, daty i budżet.
    e.  Wywołuje zewnętrzny serwis AI (`IOpenRouterService`) w celu wygenerowania 3 propozycji planów.
    f.  Przetwarza odpowiedź AI, wydobywając tytuły i treści planów.
    g.  Tworzy 3 nowe instancje encji `Plan` z danymi z AI, statusem 'Generated', ID użytkownika, datami i budżetem.
    h.  Zapisuje nowe encje `Plan` w bazie danych za pomocą `ApplicationDbContext.SaveChangesAsync()`.
    i.  Mapuje zapisane encje `Plan` na listę DTO `PlanProposalResponse`.
6.  `PlanGenerationService` zwraca listę DTO do kontrolera.
7.  Kontroler zwraca odpowiedź `201 Created` z listą DTO w ciele odpowiedzi.

## 5. Względy bezpieczeństwa
- **Uwierzytelnianie:** Punkt końcowy musi być zabezpieczony atrybutem `[Authorize]`. Należy użyć standardowych mechanizmów ASP.NET Core Identity do weryfikacji tokena użytkownika.
- **Autoryzacja:** Po pobraniu notatki z bazy danych, należy *bezpośrednio* zweryfikować, czy `Note.UserId` jest równe ID aktualnie zalogowanego użytkownika. W przeciwnym razie zwrócić `403 Forbidden`.
- **Walidacja danych wejściowych:** Rygorystyczna walidacja (`noteId`, `startDate`, `endDate`, `budget`) na poziomie modelu (`Required`, `Range`, typy danych) oraz dodatkowa walidacja w serwisie (`endDate > startDate`, istnienie notatki, przynależność notatki do użytkownika, dostępność budżetu).
- **Zarządzanie kluczami API:** Klucz API do Openrouter.ai musi być przechowywany bezpiecznie (np. w User Secrets, Azure Key Vault, zmiennych środowiskowych), a nie w kodzie źródłowym.
- **Ochrona przed nadużyciami:** Rozważyć wprowadzenie mechanizmów ograniczania liczby żądań (rate limiting) w przyszłości, aby zapobiec nadmiernemu wykorzystaniu zasobów AI.

## 6. Obsługa błędów
Należy użyć `ILogger` do rejestrowania błędów.
- **`400 Bad Request`:** Błędy walidacji modelu (brakujące pola, złe formaty, `endDate <= startDate`, ujemny `budget`). Zwracane automatycznie przez ASP.NET Core lub jawnie z kontrolera/serwisu.
- **`401 Unauthorized`:** Brak lub nieprawidłowy token uwierzytelniający. Obsługiwane przez middleware uwierzytelniający.
- **`403 Forbidden`:** Użytkownik próbuje użyć notatki, która do niego nie należy. Sprawdzane w serwisie.
- **`404 Not Found`:** Notatka o podanym `noteId` nie istnieje w bazie danych. Sprawdzane w serwisie.
- **`422 Unprocessable Entity`:** (Alternatywa dla 400/500) Logika biznesowa nie może być wykonana pomimo poprawnych danych wejściowych (np. brak możliwości ustalenia budżetu - ani w żądaniu, ani w profilu).
- **`500 Internal Server Error`:** Błędy podczas zapisu do bazy danych (`DbUpdateException`), nieobsłużone wyjątki w logice serwisu lub kontrolera.
- **`503 Service Unavailable`:** Błąd komunikacji z Openrouter.ai (timeout, błąd API, niedostępność usługi). W miarę możliwości dołączyć szczegóły błędu z odpowiedzi AI w ciele odpowiedzi serwera, zgodnie ze specyfikacją. Należy zaimplementować odpowiednią obsługę wyjątków wokół wywołania `IOpenRouterService`.

## 7. Rozważania dotyczące wydajności
- **Wywołanie zewnętrzne AI:** Jest to potencjalnie najdłużej trwająca operacja. Wywołanie powinno być asynchroniczne (`async/await`). Należy ustawić rozsądny timeout dla klienta HTTP komunikującego się z Openrouter.ai.
- **Zapytania do bazy danych:** Upewnić się, że pobieranie `Note` i `UserProfile` (z wymaganymi danymi powiązanymi: `Interests`, `TravelStyle`, `Intensity`) jest zoptymalizowane. Użyć `Include()` i `ThenInclude()` w Entity Framework Core, aby uniknąć problemu N+1 zapytań. Rozważyć użycie `AsNoTracking()` dla odczytów, jeśli encje nie będą modyfikowane (choć tutaj zapisujemy `Plan`, więc kontekst musi śledzić zmiany).
- **Przetwarzanie odpowiedzi AI:** Parsowanie odpowiedzi AI powinno być wydajne.
- **Operacje zapisu:** Zapis trzech encji `Plan` powinien odbywać się w ramach jednej transakcji (`SaveChangesAsync()` domyślnie).

## 8. Etapy wdrożenia
1.  **Utworzenie modeli:** Zdefiniować klasy `GeneratePlanProposalRequest` i `PlanProposalResponse`.
2.  **Utworzenie serwisu:** Stworzyć interfejs `IPlanGenerationService` i jego implementację `PlanGenerationService`. Wstrzyknąć zależności (`ApplicationDbContext`, `UserManager<ApplicationUser>`, `ILogger`, `IOpenRouterService`).
3.  **Implementacja logiki serwisu:**
    a.  Zaimplementować pobieranie danych użytkownika i notatki.
    b.  Dodać logikę walidacji (przynależność notatki, sprawdzanie dat, ustalanie budżetu).
    c.  Zaimplementować konstrukcję promptu dla AI.
    d.  Zintegrować wywołanie `IOpenRouterService` (zakładając, że ten serwis lub jego interfejs już istnieje lub zostanie stworzony).
    e.  Dodać obsługę błędów i timeoutów dla wywołania AI.
    f.  Zaimplementować logikę przetwarzania odpowiedzi AI.
    g.  Zaimplementować tworzenie i zapisywanie encji `Plan`.
    h.  Dodać mapowanie encji `Plan` na `PlanProposalResponse` (np. przy użyciu AutoMapper lub ręcznie).
4.  **Utworzenie kontrolera:** Stworzyć `PlanProposalsController` (lub dodać endpoint do istniejącego kontrolera).
5.  **Implementacja endpointu:**
    a.  Dodać metodę akcji `POST` z odpowiednią ścieżką (`/plan-proposals`).
    b.  Zastosować atrybut `[Authorize]`.
    c.  Wstrzyknąć `IPlanGenerationService` i `ILogger`.
    d.  Pobrać ID użytkownika.
    e.  Wywołać metodę serwisu.
    f.  Obsłużyć potencjalne wyjątki z serwisu i zwrócić odpowiednie kody statusu (`400`, `403`, `404`, `500`, `503`).
    g.  Zwrócić `201 Created` z listą `PlanProposalResponse` w przypadku sukcesu.
6.  **Konfiguracja:** Upewnić się, że `IOpenRouterService` jest poprawnie zarejestrowany w kontenerze DI i skonfigurowany (w tym klucz API).
7.  **Testowanie:** Napisać testy jednostkowe dla `PlanGenerationService` (mockując zależności) oraz testy integracyjne dla endpointu API.
8.  **Logowanie:** Zweryfikować, czy logi są poprawnie zapisywane dla różnych scenariuszy (sukces, błędy walidacji, błędy AI, błędy bazy danych). 