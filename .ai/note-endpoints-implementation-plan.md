# API Endpoint Implementation Plan: Notes CRUD (/api/v1/notes)

## 1. Przegląd punktu końcowego
Ten zestaw punktów końcowych zapewnia pełne operacje CRUD (Create, Read, Update, Delete) dla zasobów `Note` należących do uwierzytelnionego użytkownika. Obejmuje listowanie notatek z paginacją i sortowaniem, tworzenie nowych notatek, pobieranie szczegółów pojedynczej notatki, aktualizowanie istniejącej notatki i usuwanie notatki.

## 2. Szczegóły żądania

**A. Get Notes List**
- **Metoda HTTP:** `GET`
- **Struktura URL:** `/api/v1/notes`
- **Parametry:**
  - Wymagane: Brak
  - Opcjonalne (Query):
    - `page` (int, default: 1): Numer strony.
    - `pageSize` (int, default: 10): Liczba elementów na stronie.
    - `sortBy` (string, default: "modifiedAt"): Pole sortowania (`createdAt` lub `modifiedAt`).
    - `sortDirection` (string, default: "desc"): Kierunek sortowania (`asc` lub `desc`).
- **Request Body:** Brak

**B. Create Note**
- **Metoda HTTP:** `POST`
- **Struktura URL:** `/api/v1/notes`
- **Parametry:** Brak
- **Request Body:**
  ```json
  {
    "title": "String (Required, Max 100)",
    "content": "String (Required, Max 2000)"
  }
  ```

**C. Get Note Details**
- **Metoda HTTP:** `GET`
- **Struktura URL:** `/api/v1/notes/{id}`
- **Parametry:**
  - Wymagane (Path): `id` (guid) - ID notatki.
  - Opcjonalne: Brak
- **Request Body:** Brak

**D. Update Note**
- **Metoda HTTP:** `PUT`
- **Struktura URL:** `/api/v1/notes/{id}`
- **Parametry:**
  - Wymagane (Path): `id` (guid) - ID notatki.
  - Opcjonalne: Brak
- **Request Body:**
  ```json
  {
    "title": "String (Required, Max 100)",
    "content": "String (Required, Max 2000)"
  }
  ```

**E. Delete Note**
- **Metoda HTTP:** `DELETE`
- **Struktura URL:** `/api/v1/notes/{id}`
- **Parametry:**
  - Wymagane (Path): `id` (guid) - ID notatki.
  - Opcjonalne: Brak
- **Request Body:** Brak

## 3. Wykorzystywane typy
- **Modele żądań/zapytań:**
  - `GetNotesListQuery` (mapowane z parametrów query `GET /notes`)
  - `CreateNoteRequest` (mapowane z body `POST /notes`)
  - `UpdateNoteRequest` (mapowane z body `PUT /notes/{id}`)
- **DTO odpowiedzi:**
  - `NoteListItemDto` (dla elementów w liście `GET /notes`)
  - `NoteDto` (dla `GET /notes/{id}`, `POST /notes`, `PUT /notes/{id}`)
  - `PaginatedListDto<NoteListItemDto>` (dla odpowiedzi `GET /notes`)
- **Encje:**
  - `Note` (z `10xVibeTravels/Data/Note.cs`)
  - `ApplicationUser` (z `10xVibeTravels/Data/ApplicationUser.cs`, używane do pobrania `UserId`)
- **Kontekst DB:**
  - `ApplicationDbContext` (z `10xVibeTravels/Data/ApplicationDbContext.cs`)

## 4. Szczegóły odpowiedzi

**A. Get Notes List (`GET /notes`)**
- **Success:** `200 OK`
  ```json
  {
    "items": [
      {
        "id": "guid",
        "title": "Note Title",
        "contentPreview": "First 150 chars of content...", // Wygenerowane przez API
        "createdAt": "datetime",
        "modifiedAt": "datetime"
      }
      // ...
    ],
    "page": 1,
    "pageSize": 10,
    "totalItems": 50,
    "totalPages": 5
  }
  ```
- **Errors:** `400 Bad Request`, `401 Unauthorized`, `500 Internal Server Error`

**B. Create Note (`POST /notes`)**
- **Success:** `201 Created` (z nagłówkiem `Location: /api/v1/notes/{new_id}`)
  ```json
  {
    "id": "guid",
    "title": "New Note Title",
    "content": "Note content details...",
    "createdAt": "datetime", // Ustawione przez serwer
    "modifiedAt": "datetime" // Ustawione przez serwer
  }
  ```
- **Errors:** `400 Bad Request`, `401 Unauthorized`, `500 Internal Server Error`

**C. Get Note Details (`GET /notes/{id}`)**
- **Success:** `200 OK`
  ```json
  {
    "id": "guid",
    "title": "Note Title",
    "content": "Full note content...",
    "createdAt": "datetime",
    "modifiedAt": "datetime"
  }
  ```
- **Errors:** `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`

**D. Update Note (`PUT /notes/{id}`)**
- **Success:** `200 OK`
  ```json
  {
    "id": "guid", // To samo ID co w URL
    "title": "Updated Note Title",
    "content": "Updated note content...",
    "createdAt": "datetime", // Oryginalna data utworzenia
    "modifiedAt": "datetime" // Zaktualizowana przez serwer
  }
  ```
- **Errors:** `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`

**E. Delete Note (`DELETE /notes/{id}`)**
- **Success:** `204 No Content` (Brak ciała odpowiedzi)
- **Errors:** `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`

## 5. Przepływ danych

1.  Żądanie API trafia do kontrolera ASP.NET Core (`NotesController`).
2.  Uwierzytelnianie jest weryfikowane przez middleware ASP.NET Core Identity. `UserId` jest pobierane z kontekstu użytkownika.
3.  Kontroler mapuje parametry ścieżki/zapytania i/lub ciało żądania do odpowiedniego modelu (`GetNotesListQuery`, `CreateNoteRequest`, `UpdateNoteRequest`) lub pobiera `id`.
4.  Walidacja (np. FluentValidation) jest uruchamiana dla modeli żądań/zapytań. Jeśli walidacja nie powiodła się, zwracany jest błąd `400 Bad Request`.
5.  Kontroler wywołuje odpowiednią metodę w `INoteService`, przekazując `UserId` i zwalidowane dane/parametry.
6.  `NoteService`:
    *   Interakcje z `ApplicationDbContext` (EF Core) w celu wykonania operacji na bazie danych (query, insert, update, delete).
    *   **Kluczowe:** Wszystkie zapytania do bazy danych muszą filtrować po `UserId`, aby zapewnić, że użytkownik operuje tylko na swoich danych.
    *   W przypadku operacji odczytu (GET), pobiera dane i mapuje encje `Note` na odpowiednie DTO (`NoteListItemDto` lub `NoteDto`). Dla `NoteListItemDto` skraca pole `Content` do 150 znaków (`ContentPreview`).
    *   W przypadku operacji zapisu (POST, PUT, DELETE), wykonuje zmiany w bazie danych. Dla `POST` i `PUT` ustawia `CreatedAt`/`ModifiedAt`.
    *   Rzuca niestandardowe wyjątki (np. `NoteNotFoundException`, `NoteAccessDeniedException`) w przypadku błędów (np. brak rekordu, próba dostępu do cudzej notatki), które zostaną przechwycone przez middleware obsługi błędów.
7.  Kontroler odbiera wynik (dane lub potwierdzenie) lub wyjątek z `NoteService`.
8.  W przypadku sukcesu, kontroler formatuje odpowiedź HTTP z odpowiednim kodem statusu (200, 201, 204) i ciałem (jeśli dotyczy), mapując dane z serwisu na DTO odpowiedzi, jeśli jest to konieczne.
9.  W przypadku oczekiwanych błędów (np. Not Found, Forbidden z `NoteService`), middleware obsługi błędów mapuje je na odpowiednie kody statusu (404, 403).
10. W przypadku nieoczekiwanych błędów (np. błąd bazy danych), globalny handler wyjątków loguje błąd i zwraca `500 Internal Server Error`.

## 6. Względy bezpieczeństwa
- **Uwierzytelnianie:** Wszystkie punkty końcowe muszą być chronione atrybutem `[Authorize]`. Należy używać standardowego mechanizmu ASP.NET Core Identity do zarządzania sesjami/tokenami użytkowników.
- **Autoryzacja:** Warstwa serwisowa (`NoteService`) *musi* rygorystycznie weryfikować, czy `UserId` powiązany z żądaniem pasuje do `UserId` notatki, na której wykonywana jest operacja (GET by ID, PUT, DELETE). Zapobiega to atakom typu IDOR. W przypadku niezgodności należy zwrócić `403 Forbidden`.
- **Walidacja danych wejściowych:** Stosować walidację (FluentValidation) dla wszystkich danych wejściowych użytkownika (parametry query, ciało żądania), aby zapobiec nieprawidłowym danym i potencjalnym atakom (np. przez ograniczenie długości pól `title` i `content`). EF Core zapewnia ochronę przed SQL Injection.
- **Ograniczenie szybkości (Rate Limiting):** Rozważyć implementację rate limitingu (np. za pomocą `Microsoft.AspNetCore.RateLimiting`), aby zapobiec nadużyciom API.
- **Bezpieczeństwo nagłówków:** Upewnić się, że odpowiednie nagłówki bezpieczeństwa HTTP (np. `X-Content-Type-Options`, `X-Frame-Options`) są ustawione przez aplikację.

## 7. Obsługa błędów
- Zaimplementować globalny middleware obsługi wyjątków.
- Middleware powinien przechwytywać wyjątki z warstwy serwisowej i mapować je na odpowiednie kody stanu HTTP:
  - Niestandardowy `NoteNotFoundException` -> `404 Not Found`
  - Niestandardowy `NoteAccessDeniedException` (lub podobny dla niezgodności UserId) -> `403 Forbidden`
  - `ValidationException` z FluentValidation (jeśli nie obsłużony przez middleware ASP.NET) -> `400 Bad Request`
  - Inne nieoczekiwane wyjątki (np. `DbUpdateException`, `NullReferenceException`) -> Logowanie szczegółów błędu i zwrot generycznego `500 Internal Server Error` z minimalną ilością informacji dla klienta.
- Błędy walidacji parametrów query (`GET /notes`) powinny być obsługiwane w kontrolerze lub przez dedykowany filtr akcji, zwracając `400 Bad Request`.

## 8. Rozważania dotyczące wydajności
- **Paginacja:** `GET /notes` musi używać `Skip()` i `Take()` w zapytaniu EF Core, aby pobierać tylko potrzebną stronę danych, a nie całą tabelę.
- **Sortowanie:** Sortowanie powinno być stosowane na poziomie bazy danych (w zapytaniu EF Core), a nie w pamięci.
- **Indeksy:** Upewnić się, że istnieją odpowiednie indeksy w bazie danych dla pól używanych w klauzulach `WHERE` i `ORDER BY` (zgodnie z `.ai/db-plan.md`: `Notes(UserId)`, `Notes(ModifiedAt DESC)`, `Notes(CreatedAt DESC)`).
- **Projekcja:** Używać `Select()` w EF Core, aby pobierać tylko wymagane kolumny z bazy danych, zwłaszcza dla `GET /notes`, gdzie pobieramy tylko podgląd treści (`ContentPreview`). Unikać pobierania całej encji `Note`, jeśli nie jest to konieczne.
- **Asynchroniczność:** Wszystkie operacje I/O (głównie interakcje z bazą danych) muszą być asynchroniczne (`async`/`await`), aby nie blokować wątków serwera.

## 9. Etapy wdrożenia
1.  **Modele DTO i Żądań:** Zdefiniować klasy C# dla `GetNotesListQuery`, `CreateNoteRequest`, `UpdateNoteRequest`, `NoteListItemDto`, `NoteDto`, `PaginatedListDto<T>` w odpowiednim folderze projektu (np. `Application/Notes/DTOs`).
2.  **Walidacja:** Zaimplementować walidatory FluentValidation dla `GetNotesListQuery`, `CreateNoteRequest`, `UpdateNoteRequest`. Skonfigurować FluentValidation w `Startup.cs` lub `Program.cs`.
3.  **Interfejs Serwisu:** Zdefiniować interfejs `INoteService` z metodami odpowiadającymi operacjom CRUD (np. `GetNotesAsync`, `CreateNoteAsync`, `GetNoteByIdAsync`, `UpdateNoteAsync`, `DeleteNoteAsync`).
4.  **Implementacja Serwisu:** Stworzyć klasę `NoteService` implementującą `INoteService`. Wstrzyknąć `ApplicationDbContext`. Zaimplementować logikę dla każdej metody, w tym:
    *   Pobieranie `UserId` (np. z `IHttpContextAccessor`).
    *   Filtrowanie po `UserId`.
    *   Operacje EF Core (zapytania, dodawanie, aktualizowanie, usuwanie) używając `async`/`await`.
    *   Mapowanie między encjami a DTO (rozważyć użycie AutoMapper lub mapowanie ręczne).
    *   Implementacja logiki `ContentPreview`.
    *   Rzucanie niestandardowych wyjątków dla błędów 403/404.
5.  **Rejestracja Serwisu:** Zarejestrować `INoteService` i `NoteService` w kontenerze DI.
6.  **Kontroler:** Stworzyć `NotesController` dziedziczący po `ControllerBase` lub niestandardowej bazie kontrolerów API.
    *   Wstrzyknąć `INoteService`.
    *   Dodać atrybut `[Route("api/v1/notes")]` i `[ApiController]`.
    *   Dodać atrybut `[Authorize]` na poziomie kontrolera.
    *   Zaimplementować metody akcji dla każdego punktu końcowego (`GET`, `POST`, `GET {id}`, `PUT {id}`, `DELETE {id}`), używając odpowiednich atrybutów HTTP (`[HttpGet]`, `[HttpPost]`, etc.) i routingu (`[HttpGet("{id}")]`).
    *   Mapować parametry żądania do modeli/zmiennych.
    *   Wywoływać odpowiednie metody `INoteService`.
    *   Zwracać odpowiednie `IActionResult` (np. `Ok()`, `CreatedAtAction()`, `NoContent()`, `NotFound()`, `Forbid()`, `BadRequest()`).
7.  **Obsługa Błędów:** Skonfigurować lub zweryfikować middleware globalnej obsługi wyjątków, aby mapował niestandardowe wyjątki i inne błędy na odpowiedzi HTTP zgodnie z sekcją "Obsługa błędów".
8.  **Konfiguracja:** Upewnić się, że wszystkie zależności (EF Core, Identity, FluentValidation, Logging) są poprawnie skonfigurowane. 