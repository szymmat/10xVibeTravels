# API Endpoint Implementation Plan: /api/v1/plans

## 1. Przegląd punktu końcowego
Ten zestaw punktów końcowych zapewnia funkcjonalność CRUD (Create - nie dotyczy tego planu, Read, Update, Delete) dla zasobów `Plan` powiązanych z uwierzytelnionym użytkownikiem. Umożliwia użytkownikom przeglądanie wygenerowanych propozycji planów oraz zapisanych (zaakceptowanych) planów, przeglądanie ich szczegółów, aktualizowanie statusu/tytułu propozycji lub treści zapisanych planów oraz usuwanie planów.

## 2. Szczegóły żądania

### 2.1. Get Plans List (`GET /api/v1/plans`)
- **Metoda HTTP:** `GET`
- **Struktura URL:** `/api/v1/plans`
- **Parametry:**
  - **Wymagane:** Brak
  - **Opcjonalne (Query):**
    - `status` (string): Filtruje plany według statusu. Dozwolone wartości: "Generated", "Accepted", "Rejected". Domyślnie: "Accepted".
    - `page` (int): Numer strony (>= 1). Domyślnie: 1.
    - `pageSize` (int): Liczba elementów na stronie (>= 1). Domyślnie: 10.
    - `sortBy` (string): Pole do sortowania. Dozwolone wartości: "createdAt", "modifiedAt". Domyślnie: "modifiedAt".
    - `sortDirection` (string): Kierunek sortowania. Dozwolone wartości: "asc", "desc". Domyślnie: "desc".
- **Request Body:** Brak

### 2.2. Get Plan Details (`GET /api/v1/plans/{id}`)
- **Metoda HTTP:** `GET`
- **Struktura URL:** `/api/v1/plans/{id}`
- **Parametry:**
  - **Wymagane (Path):** `id` (guid): Identyfikator planu.
  - **Opcjonalne:** Brak
- **Request Body:** Brak

### 2.3. Update Plan (`PATCH /api/v1/plans/{id}`)
- **Metoda HTTP:** `PATCH`
- **Struktura URL:** `/api/v1/plans/{id}`
- **Parametry:**
  - **Wymagane (Path):** `id` (guid): Identyfikator planu.
  - **Opcjonalne:** Brak
- **Request Body:** Obiekt JSON zawierający pola do aktualizacji. Dozwolone pola zależą od aktualnego statusu planu:
  - Jeśli `Status` to "Generated": Można podać `status` ("Accepted" lub "Rejected") LUB `title` (string, max 100 znaków).
  - Jeśli `Status` to "Accepted": Można podać `content` (string).
  - Przykłady:
    ```json
    { "status": "Accepted" }
    ```
    ```json
    { "title": "Nowy Tytuł Planu" }
    ```
    ```json
    { "content": "Zaktualizowana treść planu..." }
    ```
  - Jednoczesna aktualizacja wielu pól w jednym żądaniu PATCH jest dozwolona, o ile jest zgodna z powyższymi regułami (np. nie można jednocześnie zaktualizować `title` i `content`).

### 2.4. Delete Plan (`DELETE /api/v1/plans/{id}`)
- **Metoda HTTP:** `DELETE`
- **Struktura URL:** `/api/v1/plans/{id}`
- **Parametry:**
  - **Wymagane (Path):** `id` (guid): Identyfikator planu.
  - **Opcjonalne:** Brak
- **Request Body:** Brak

## 3. Wykorzystywane typy
- **Encje:**
  - `_10xVibeTravels.Data.Plan`
  - `_10xVibeTravels.Data.ApplicationUser` (do relacji)
- **Enum:**
  - `_10xVibeTravels.Data.PlanStatus` (używane w DTOs dla typowania)
- **DTOs:**
  - `PlanListQueryParameters` (dla `GET /plans`): Zawiera `Status` (string), `Page` (int), `PageSize` (int), `SortBy` (string), `SortDirection` (string).
  - `PlanListItemDto` (dla listy w `GET /plans`): `Id` (Guid), `Status` (PlanStatus), `Title` (string), `ContentPreview` (string), `StartDate` (DateTime), `EndDate` (DateTime), `Budget` (decimal), `CreatedAt` (DateTime), `ModifiedAt` (DateTime).
  - `PaginatedResult<T>` (dla odpowiedzi `GET /plans`): `Items` (List<T>), `Page` (int), `PageSize` (int), `TotalItems` (int), `TotalPages` (int).
  - `PlanDetailDto` (dla `GET /plans/{id}` i odpowiedzi `PATCH`): `Id` (Guid), `Status` (PlanStatus), `Title` (string), `Content` (string), `StartDate` (DateTime), `EndDate` (DateTime), `Budget` (decimal), `CreatedAt` (DateTime), `ModifiedAt` (DateTime).
  - `UpdatePlanDto` (dla body `PATCH /plans/{id}`): Zawiera nullable `Status` (PlanStatus?), `Title` (string?), `Content` (string?).
- **Modele Walidacji (FluentValidation):**
  - `PlanListQueryParametersValidator`
  - `UpdatePlanDtoValidator` (dla podstawowych reguł jak długość `Title`)
- **Mapping:** Konfiguracja AutoMapper lub mapowanie ręczne do konwersji między `Plan` (string Status) a DTOs (`PlanStatus` enum) oraz generowania `ContentPreview`.

## 4. Szczegóły odpowiedzi

### 4.1. Get Plans List (`GET /api/v1/plans`)
- **Sukces (`200 OK`):**
  ```json
  {
    "items": [
      {
        "id": "guid",
        "status": "Accepted", // Enum string representation
        "title": "Plan Title",
        "contentPreview": "First 150 chars of content...",
        "startDate": "YYYY-MM-DD",
        "endDate": "YYYY-MM-DD",
        "budget": 1200.50,
        "createdAt": "datetime",
        "modifiedAt": "datetime"
      }
      // ...
    ],
    "page": 1,
    "pageSize": 10,
    "totalItems": 25,
    "totalPages": 3
  }
  ```
- **Błędy:** `400 Bad Request`, `401 Unauthorized`, `500 Internal Server Error`.

### 4.2. Get Plan Details (`GET /api/v1/plans/{id}`)
- **Sukces (`200 OK`):**
  ```json
  {
    "id": "guid",
    "status": "Accepted", // Enum string representation
    "title": "Plan Title",
    "content": "Full plan content...",
    "startDate": "YYYY-MM-DD",
    "endDate": "YYYY-MM-DD",
    "budget": 1200.50,
    "createdAt": "datetime",
    "modifiedAt": "datetime"
  }
  ```
- **Błędy:** `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`.

### 4.3. Update Plan (`PATCH /api/v1/plans/{id}`)
- **Sukces (`200 OK`):** Odpowiedź zawiera zaktualizowany obiekt `PlanDetailDto` (jak w `GET /plans/{id}`).
- **Błędy:** `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`.

### 4.4. Delete Plan (`DELETE /api/v1/plans/{id}`)
- **Sukces (`204 No Content`):** Brak ciała odpowiedzi.
- **Błędy:** `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`.

## 5. Przepływ danych
1.  Żądanie API trafia do odpowiedniej metody w `PlansController`.
2.  Middleware ASP.NET Core obsługuje uwierzytelnianie i autoryzację. Identyfikator użytkownika (`userId`) jest pobierany z `HttpContext.User`.
3.  Kontroler waliduje parametry ścieżki/zapytania oraz ciało żądania (przy użyciu walidatorów FluentValidation dla podstawowych reguł).
4.  Kontroler wywołuje odpowiednią metodę w `IPlanService`, przekazując `userId` i zwalidowane parametry/DTO.
5.  `PlanService` wykonuje logikę biznesową:
    - Interakcja z `ApplicationDbContext` (EF Core) w celu pobrania/modyfikacji danych (`Plan`).
    - **Ważne:** Wszystkie zapytania do bazy danych muszą filtrować wyniki po `userId`, aby zapewnić, że użytkownik operuje tylko na swoich danych.
    - Sprawdzanie własności zasobu (`userId` musi pasować do `Plan.UserId`). Jeśli nie pasuje -> zwróć błąd (403/404).
    - Generowanie `ContentPreview` (pierwsze 150 znaków `Content`) dla `PlanListItemDto`.
    - Mapowanie między encją `Plan` (string `Status`) a DTOs (`PlanStatus` enum).
    - Weryfikacja reguł biznesowych dla `PATCH` (dozwolone zmiany w zależności od statusu, prawidłowe przejścia statusu).
    - Obsługa paginacji, sortowania i filtrowania dla `GET /plans`.
    - Aktualizacja pól `ModifiedAt` przy operacjach `PATCH`.
    - Zapis zmian do bazy danych (`_context.SaveChangesAsync()`).
6.  `PlanService` zwraca wynik (DTO lub status powodzenia/błędu) do `PlansController`.
7.  `PlansController` mapuje wynik z serwisu na odpowiedni `ActionResult` (np. `Ok()`, `NotFound()`, `Forbid()`, `BadRequest()`, `NoContent()`) z odpowiednim kodem statusu i ciałem odpowiedzi (jeśli dotyczy).

## 6. Względy bezpieczeństwa
- **Uwierzytelnianie:** Wszystkie punkty końcowe muszą być zabezpieczone atrybutem `[Authorize]`. System tożsamości ASP.NET Core (Identity) obsłuży weryfikację tokenów/ciasteczek.
- **Autoryzacja:** Kluczowe jest egzekwowanie własności zasobów. Każda operacja na konkretnym planie (`GET {id}`, `PATCH {id}`, `DELETE {id}`) musi weryfikować, czy `Plan.UserId` pasuje do `userId` zalogowanego użytkownika. `GET /plans` musi automatycznie filtrować po `userId`. Niewłaściwa autoryzacja prowadzi do zwrócenia `403 Forbidden` lub `404 Not Found`.
- **Walidacja danych wejściowych:** Użycie FluentValidation do sprawdzania parametrów zapytania i ciała żądania zapobiega błędom przetwarzania i potencjalnym atakom (np. injection przez nieprawidłowe dane). Sprawdzanie długości `Title`, poprawności wartości enum/string dla `status`, `sortBy`, `sortDirection`.
- **Ochrona przed Mass Assignment:** Użycie dedykowanych DTOs (`UpdatePlanDto`) zapobiega nadpisaniu nieautoryzowanych pól encji.
- **Zabezpieczenia EF Core:** Użycie parametryzowanych zapytań przez EF Core chroni przed atakami SQL Injection.
- **CSRF:** Jeśli używane są ciasteczka do uwierzytelniania (np. w Blazor Server z formularzami tradycyjnymi), należy zastosować mechanizmy ochrony przed CSRF (np. antiforgery tokens). Dla API bezstanowego (np. z tokenami JWT) ryzyko jest mniejsze, ale należy stosować odpowiednie nagłówki CORS.
- **Rate Limiting:** Rozważyć implementację ograniczania liczby żądań (rate limiting) w celu ochrony przed atakami typu DoS/brute-force.

## 7. Obsługa błędów
- **Błędy Walidacji (400 Bad Request):** Zwracane automatycznie przez pipeline ASP.NET Core przy użyciu FluentValidation lub manualnie z kontrolera/serwisu dla błędów logiki biznesowej (np. nieprawidłowe przejście statusu). Odpowiedź powinna zawierać szczegóły błędu.
- **Brak Uwierzytelnienia (401 Unauthorized):** Zwracane przez middleware uwierzytelniający, jeśli użytkownik nie jest zalogowany.
- **Brak Autoryzacji (403 Forbidden):** Zwracane, gdy uwierzytelniony użytkownik próbuje uzyskać dostęp do zasobu, który do niego nie należy.
- **Nie znaleziono zasobu (404 Not Found):** Zwracane, gdy żądany plan o podanym `id` nie istnieje w bazie danych (lub nie należy do użytkownika, w zależności od strategii - preferowane 404, jeśli nie znaleziono dla danego użytkownika).
- **Błędy Wewnętrzne Serwera (500 Internal Server Error):** Obsługiwane przez globalny middleware obsługi wyjątków, który loguje szczegóły błędu i zwraca generyczną odpowiedź 500. Należy unikać ujawniania szczegółów implementacyjnych w odpowiedziach błędów.

## 8. Rozważania dotyczące wydajności
- **Zapytania do Bazy Danych:**
  - Używaj `AsNoTracking()` dla zapytań tylko do odczytu (`GET /plans`, `GET /plans/{id}`), aby uniknąć narzutu śledzenia zmian przez EF Core.
  - Pobieraj tylko niezbędne dane. Dla `GET /plans` użyj `Select()` do mapowania na `PlanListItemDto` w zapytaniu SQL, aby uniknąć pobierania pełnego `Content`.
  - Zapewnij istnienie odpowiednich indeksów w bazie danych (zgodnie z `db-plan.md`), zwłaszcza na `UserId`, `Status`, `CreatedAt`, `ModifiedAt`.
  - Implementuj paginację po stronie serwera (`Skip()`, `Take()`) - nie pobieraj wszystkich pasujących rekordów do pamięci.
- **Mapowanie DTO:** Użycie zoptymalizowanej biblioteki mapującej (np. AutoMapper z prekompilowanymi mapowaniami) lub wydajnego mapowania ręcznego.
- **Generowanie `ContentPreview`:** Wykonywane w serwisie po pobraniu danych; upewnij się, że jest to robione efektywnie (np. `Substring` z odpowiednim sprawdzaniem długości).
- **Asynchroniczność:** Wszystkie operacje I/O (głównie bazodanowe) muszą być asynchroniczne (`async`/`await`) w całej ścieżce wywołania (kontroler -> serwis -> EF Core), aby nie blokować wątków serwera.

## 9. Etapy wdrożenia
1.  **Definicja Modeli i DTOs:**
    - Utwórz/zweryfikuj klasy DTO: `PlanListQueryParameters`, `PlanListItemDto`, `PaginatedResult<T>`, `PlanDetailDto`, `UpdatePlanDto`.
    - Upewnij się, że enum `PlanStatus` istnieje i jest używany w DTOs.
2.  **Konfiguracja EF Core:**
    - Zweryfikuj, czy `ApplicationDbContext` poprawnie konfiguruje encję `Plan` zgodnie ze schematem `db-plan.md` (typy kolumn, klucze obce, indeksy, ograniczenia, wartości domyślne). Nie jest potrzebna konwersja enuma Status.
3.  **Utworzenie Serwisu (`PlanService`):**
    - Zdefiniuj interfejs `IPlanService`.
    - Zaimplementuj `PlanService` z wstrzykniętym `ApplicationDbContext` i `IMapper` (jeśli używany jest AutoMapper).
    - Zaimplementuj metody serwisu (`GetPlansAsync`, `GetPlanByIdAsync`, `UpdatePlanAsync`, `DeletePlanAsync`), uwzględniając filtrowanie po `userId`, logikę biznesową, mapowanie, generowanie `ContentPreview`, obsługę błędów (zwracanie null/bool lub rzucanie specyficznych wyjątków).
4.  **Rejestracja Zależności:** Zarejestruj `IPlanService` i `PlanService` w kontenerze DI (np. `services.AddScoped<IPlanService, PlanService>();`). Zarejestruj AutoMapper (jeśli używany).
5.  **Konfiguracja Mapowania:** Skonfiguruj profile AutoMapper lub utwórz metody mapujące dla `Plan` -> `PlanListItemDto`, `Plan` -> `PlanDetailDto`. Uwzględnij mapowanie `string Status` <-> `PlanStatus` enum i generowanie `ContentPreview`.
6.  **Implementacja Kontrolera (`PlansController`):**
    - Utwórz `PlansController` dziedziczący z `ControllerBase`.
    - Dodaj atrybut `[Route("api/v1/plans")]` i `[ApiController]`.
    - Wstrzyknij `IPlanService`.
    - Zaimplementuj metody akcji dla każdego endpointu (`GetPlans`, `GetPlan`, `UpdatePlan`, `DeletePlan`).
    - Dodaj atrybut `[Authorize]` do kontrolera lub poszczególnych metod.
    - Dodaj atrybuty metod HTTP (`[HttpGet]`, `[HttpGet("{id}")]`, `[HttpPatch("{id}")]`, `[HttpDelete("{id}")]`).
    - Pobierz `userId` z `HttpContext.User`.
    - Wywołaj odpowiednie metody `IPlanService`.
    - Mapuj wyniki z serwisu na `ActionResult` (np. `Ok(dto)`, `NotFound()`, `NoContent()`, `Forbid()`, `BadRequest(ModelState)`).
7.  **Implementacja Walidacji (FluentValidation):**
    - Utwórz walidatory dla `PlanListQueryParameters` i `UpdatePlanDto`.
    - Zintegruj FluentValidation z pipeline ASP.NET Core.
    - Dodaj bardziej złożoną walidację (zależną od stanu) wewnątrz `PlanService.UpdatePlanAsync`.
8.  **Konfiguracja Middleware:** Upewnij się, że standardowe middleware (uwierzytelnianie, autoryzacja, obsługa wyjątków, routing, CORS) jest poprawnie skonfigurowane w `Program.cs` lub `Startup.cs`.
9.  **Testowanie Manualne:** Przetestuj każdy endpoint przy użyciu narzędzi takich jak Postman lub Swagger UI, sprawdzając różne scenariusze (sukces, błędy walidacji, brak autoryzacji, brak zasobu, błędy serwera). 