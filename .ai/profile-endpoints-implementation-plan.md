# API Endpoint Implementation Plan: User Profile Management (/api/v1/profile)

## 1. Przegląd punktu końcowego

Ten zestaw punktów końcowych zarządza preferencjami profilu użytkownika, w tym budżetem, stylem podróży, intensywnością i zainteresowaniami. Umożliwia pobieranie, częściową aktualizację oraz pełne zastępowanie zainteresowań użytkownika. Wszystkie operacje wymagają uwierzytelnienia.

## 2. Szczegóły żądania

### A. Get User Profile
-   **Metoda HTTP:** `GET`
-   **Struktura URL:** `/api/v1/profile`
-   **Parametry:**
    -   Wymagane: Brak (kontekst użytkownika z uwierzytelnienia)
    -   Opcjonalne: Brak
-   **Request Body:** Brak

### B. Update User Profile
-   **Metoda HTTP:** `PATCH`
-   **Struktura URL:** `/api/v1/profile`
-   **Parametry:**
    -   Wymagane: Brak (kontekst użytkownika z uwierzytelnienia)
    -   Opcjonalne: Brak
-   **Request Body:**
    ```json
    {
      "budget": 1500.00, // optional, decimal or null
      "travelStyleId": "guid", // optional, guid or null
      "intensityId": "guid" // optional, guid or null
    }
    ```

### C. Set User Interests
-   **Metoda HTTP:** `PUT`
-   **Struktura URL:** `/api/v1/profile/interests`
-   **Parametry:**
    -   Wymagane: Brak (kontekst użytkownika z uwierzytelnienia)
    -   Opcjonalne: Brak
-   **Request Body:**
    ```json
    {
      "interestIds": [ // Array of Interest IDs (uniqueidentifier)
        "guid1",
        "guid2"
      ]
    }
    ```

## 3. Wykorzystywane typy

*   **DTOs (Data Transfer Objects):**
    *   `LookupDto`:
        *   `Id` (Guid)
        *   `Name` (string)
    *   `UserProfileDto`:
        *   `Budget` (decimal?)
        *   `TravelStyle` (LookupDto?)
        *   `Intensity` (LookupDto?)
        *   `Interests` (List<LookupDto>)
        *   `CreatedAt` (DateTime)
        *   `ModifiedAt` (DateTime)
*   **Command Models:**
    *   `UpdateUserProfileCommand`:
        *   `Budget` (decimal?)
        *   `TravelStyleId` (Guid?)
        *   `IntensityId` (Guid?)
    *   `SetUserInterestsCommand`:
        *   `InterestIds` (List<Guid>) - Wymagane (non-null), może być pusta lista.

## 4. Szczegóły odpowiedzi

### A. Get User Profile
-   **Success (200 OK):**
    ```json
    {
      "budget": 999.99, // decimal or null
      "travelStyle": { "id": "guid", "name": "Luksusowy" }, // object or null
      "intensity": { "id": "guid", "name": "Umiarkowany" }, // object or null
      "interests": [ { "id": "guid", "name": "Historia" } /* ... */ ], // array
      "createdAt": "datetime",
      "modifiedAt": "datetime"
    }
    ```
-   **Errors:** `401 Unauthorized`, `404 Not Found` (jeśli profil nie istnieje)

### B. Update User Profile
-   **Success (200 OK):** Zwraca zaktualizowany `UserProfileDto` (jak w GET).
-   **Errors:** `400 Bad Request`, `401 Unauthorized`

### C. Set User Interests
-   **Success (200 OK):**
    ```json
    [ // Zaktualizowana lista zainteresowań użytkownika
      { "id": "guid1", "name": "Historia" },
      { "id": "guid2", "name": "Sztuka" }
      // ...
    ]
    ```
-   **Errors:** `400 Bad Request`, `401 Unauthorized`

## 5. Przepływ danych

1.  **Żądanie:** Przychodzi do kontrolera ASP.NET Core (`ProfileController`).
2.  **Uwierzytelnienie:** Middleware ASP.NET Core Identity weryfikuje token JWT/cookie, zapewniając dostęp do `UserId`.
3.  **Autoryzacja:** Atrybut `[Authorize]` na kontrolerze/akcji zapewnia, że tylko zalogowani użytkownicy mogą uzyskać dostęp.
4.  **Walidacja (Model Binding):** ASP.NET Core waliduje strukturę payloadu (np. typy danych w `UpdateUserProfileCommand`, `SetUserInterestsCommand`). Zwraca `400 Bad Request` w przypadku błędów strukturalnych.
5.  **Przekazanie do Serwisu:** Kontroler wywołuje odpowiednią metodę w `UserProfileService`, przekazując `UserId` i dane z żądania (command models).
6.  **Logika Biznesowa (UserProfileService):**
    *   **Walidacja Semantyczna:** Serwis sprawdza, czy podane ID (np. `TravelStyleId`, `IntensityId`, `InterestIds`) istnieją w odpowiednich tabelach bazy danych (`TravelStyles`, `Intensities`, `Interests`). Zwraca wyjątek w przypadku niepowodzenia, który kontroler mapuje na `400 Bad Request`.
    *   **Interakcja z Bazą Danych:** Serwis używa `ApplicationDbContext` (EF Core) do:
        *   Pobierania danych (`UserProfiles`, `TravelStyles`, `Intensities`, `Interests`, `UserInterests`), stosując `Include`/`ThenInclude` dla powiązanych encji i filtrując zawsze po `UserId`.
        *   Tworzenia nowego `UserProfile`, jeśli nie istnieje (w `PATCH` / `PUT`).
        *   Aktualizacji pól w `UserProfiles`.
        *   Usuwania/Dodawania wpisów w `UserInterests` (dla `PUT /profile/interests`).
        *   Aktualizacji `ModifiedAt`.
    *   **Mapowanie:** Serwis mapuje encje EF Core na DTO (`UserProfileDto`, `LookupDto`) przed zwróceniem do kontrolera (np. przy użyciu AutoMapper lub ręcznego mapowania).
7.  **Odpowiedź:** Kontroler otrzymuje DTO z serwisu i zwraca je w odpowiedzi HTTP z odpowiednim kodem statusu (`200 OK`). W przypadku wyjątków złapanych przez middleware/filtry akcji, zwracane są odpowiednie kody błędów (`400`, `401`, `404`, `500`).

## 6. Względy bezpieczeństwa

*   **Uwierzytelnienie:** Wymagane dla wszystkich endpointów. Implementowane przez ASP.NET Core Identity. `UserId` musi być bezpiecznie pobierany z kontekstu uwierzytelnionego użytkownika (`HttpContext.User`).
*   **Autoryzacja:** Każda operacja w `UserProfileService` musi bezwzględnie filtrować dane po `UserId` uzyskanym z kontekstu, aby zapobiec dostępowi lub modyfikacji danych innych użytkowników.
*   **Walidacja danych wejściowych:**
    *   Walidacja struktury przez model binding ASP.NET Core.
    *   Walidacja semantyczna (np. istnienie powiązanych ID) w `UserProfileService` przed wykonaniem operacji na bazie danych. Należy zwrócić `400 Bad Request` w przypadku nieprawidłowych ID.
    *   Użycie dedykowanych Command Models (`UpdateUserProfileCommand`, `SetUserInterestsCommand`) zapobiega problemom Mass Assignment.
*   **CSRF:** Zabezpieczenia standardowe dla API (np. tokeny) zamiast tradycyjnych mechanizmów anty-CSRF opartych na ciasteczkach.
*   **Logowanie:** Logowanie błędów powinno maskować wrażliwe dane.

## 7. Obsługa błędów

*   **400 Bad Request:**
    *   Nieprawidłowa struktura JSON w ciele żądania.
    *   Błędy walidacji modelu (np. błędny typ danych dla `budget`).
    *   Nieprawidłowe ID w `UpdateUserProfileCommand` (`TravelStyleId`, `IntensityId`).
    *   Nieprawidłowe ID w `SetUserInterestsCommand` (`InterestIds`).
    *   Brak wymaganego pola `interestIds` w `PUT /profile/interests`.
    *   Odpowiedź powinna zawierać szczegóły błędu walidacji.
*   **401 Unauthorized:** Użytkownik nie jest zalogowany lub jego sesja/token wygasł. Obsługiwane przez middleware uwierzytelniania.
*   **404 Not Found:**
    *   Tylko dla `GET /profile`, jeśli profil użytkownika jeszcze nie istnieje w bazie danych.
*   **500 Internal Server Error:**
    *   Błędy połączenia z bazą danych.
    *   Niespodziewane wyjątki podczas przetwarzania w `UserProfileService`.
    *   Błędy mapowania encja -> DTO.
    *   Należy logować szczegóły błędu (stack trace, `UserId`, payload żądania - z maskowaniem).

## 8. Rozważania dotyczące wydajności

*   **Zapytania do bazy danych:**
    *   Używać `Include`/`ThenInclude` w EF Core, aby unikać problemu N+1 przy pobieraniu powiązanych danych (TravelStyle, Intensity, Interests).
    *   Dla `GET /profile`, jedno zapytanie powinno wystarczyć do pobrania wszystkich potrzebnych danych.
    *   Dla `PATCH /profile`, pobranie profilu, aktualizacja i zapis. Walidacja ID może wymagać dodatkowych zapytań `AnyAsync`, jeśli nie są one cachowane.
    *   Dla `PUT /profile/interests`, wymaga pobrania istniejących zainteresowań użytkownika (lub sprawdzenia istnienia profilu), walidacji wszystkich ID zainteresowań z `Interests` (potencjalnie jedno zapytanie `CountAsync` lub `Where().Select().ToListAsync()`), usunięcia starych powiązań (`ExecuteDeleteAsync`) i dodania nowych (`AddRangeAsync`). Należy to opakować w transakcję.
*   **Indeksy:** Upewnić się, że odpowiednie indeksy istnieją (zgodnie z `db-plan.md`), szczególnie na `UserProfiles(UserId)`, `UserInterests(UserId)`, `UserInterests(InterestId)`.
*   **Mapowanie:** Użycie zoptymalizowanego mapowania (np. skompilowane wyrażenia AutoMapper) może poprawić wydajność przy dużym obciążeniu.
*   **Caching:** Rozważyć cachowanie słowników (`TravelStyles`, `Intensities`, `Interests`) po stronie serwera, aby zredukować liczbę zapytań do bazy podczas walidacji ID.

## 9. Etapy wdrożenia

1.  **Modele:** Zdefiniować klasy DTO (`LookupDto`, `UserProfileDto`) i Command (`UpdateUserProfileCommand`, `SetUserInterestsCommand`) w odpowiednim projekcie (np. `Application` lub `Contracts`).
2.  **Serwis (Interfejs):** Zdefiniować interfejs `IUserProfileService` z metodami `GetUserProfileAsync`, `UpdateUserProfileAsync`, `SetUserInterestsAsync`.
3.  **Serwis (Implementacja):** Utworzyć klasę `UserProfileService` implementującą `IUserProfileService`. Wstrzyknąć `ApplicationDbContext`, `ILogger`. Zaimplementować logikę pobierania, tworzenia, aktualizacji, walidacji ID i mapowania encji na DTO. Użyć `async/await`. Obsłużyć przypadki, gdy profil nie istnieje. Zaimplementować logikę dla `PUT /interests` (usunięcie starych, dodanie nowych, walidacja ID, transakcja).
4.  **Rejestracja Serwisu:** Zarejestrować `IUserProfileService` i `UserProfileService` w kontenerze DI (Dependency Injection) w `Program.cs` lub odpowiednim pliku konfiguracyjnym.
5.  **Kontroler:** Utworzyć `ProfileController` dziedziczący po `ControllerBase` w projekcie `WebAPI` lub `Server`.
6.  **Wstrzyknięcie Zależności:** Wstrzyknąć `IUserProfileService` i `ILogger` do konstruktora kontrolera.
7.  **Routing i Atrybuty:** Dodać atrybuty routingu (`[Route("api/v1/profile")]`, `[ApiController]`) i autoryzacji (`[Authorize]`).
8.  **Akcje Kontrolera (GET):** Zaimplementować akcję `GetProfile()` (`[HttpGet]`). Wywołać `_userProfileService.GetUserProfileAsync(userId)`. Obsłużyć przypadek `null` (zwrócić `NotFound()`). Zwrócić `Ok(profileDto)`.
9.  **Akcje Kontrolera (PATCH):** Zaimplementować akcję `UpdateProfile([FromBody] UpdateUserProfileCommand command)` (`[HttpPatch]`). Wywołać `_userProfileService.UpdateUserProfileAsync(userId, command)`. Zwrócić `Ok(updatedProfileDto)`. Dodać obsługę wyjątków walidacji z serwisu (mapowanie na `BadRequest()`).
10. **Akcje Kontrolera (PUT):** Zaimplementować akcję `SetInterests([FromBody] SetUserInterestsCommand command)` (`[HttpPut("interests")]`). Wywołać `_userProfileService.SetUserInterestsAsync(userId, command)`. Zwrócić `Ok(interestsDtoList)`. Dodać obsługę wyjątków walidacji z serwisu (mapowanie na `BadRequest()`).
11. **Obsługa Błędów Globalnych:** Upewnić się, że istnieje globalny mechanizm obsługi wyjątków (np. middleware), który łapie nieobsłużone wyjątki, loguje je i zwraca `500 Internal Server Error`.
12. **Mapowanie (Opcjonalnie):** Skonfigurować AutoMapper, jeśli jest używany, do mapowania między encjami EF Core a DTO/Command Models.
13. **Konfiguracja Logowania:** Skonfigurować poziomy logowania i wyjścia (np. konsola, plik) zgodnie z wymaganiami projektu. 