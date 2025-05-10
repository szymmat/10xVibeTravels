# Plan implementacji widoku Dashboard (Home.razor)

## 1. Przegląd
Widok Dashboard (`Home.razor`) jest głównym punktem startowym dla zalogowanego użytkownika. Ma na celu zapewnienie szybkiego przeglądu ostatnich aktywności (notatki, plany) oraz statusu oczekujących propozycji planów podróży. Umożliwia również szybkie tworzenie nowej notatki.

## 2. Routing widoku
Widok powinien być dostępny pod główną ścieżką aplikacji dla zalogowanych użytkowników:
- **Ścieżka:** `/`
- Wymaga autoryzacji użytkownika (należy użyć dyrektywy `@attribute [Authorize]`).

## 3. Struktura komponentów
```plaintext
Home.razor (@page "/")
├── @attribute [Authorize]
├── LoadingSpinner (@bind-isLoading)
├── div class="alert alert-danger" (@if !string.IsNullOrEmpty(ErrorMessage))
├── div class="row" (@if !IsLoading && string.IsNullOrEmpty(ErrorMessage))
│   ├── div class="col-lg-4 mb-3" // Sekcja Notatek
│   │   ├── div class="card h-100" 
│   │   │   ├── div class="card-header d-flex justify-content-between align-items-center"
│   │   │   │   ├── h5 ("Ostatnie Notatki")
│   │   │   │   └── button class="btn btn-sm btn-primary" @onclick="() => ShowQuickNoteModal = true" ("+ Nowa")
│   │   │   └── div class="card-body"
│   │   │       └── RecentNotesSection (Notes="RecentNotes")
│   │   │           └── [if notes empty] EmptyState (Message="Brak notatek")
│   │   │           └── [else] ul class="list-group list-group-flush"
│   │   │               └── foreach note in Notes
│   │   │                   └── li class="list-group-item"
│   │   │                       └── NavLink href=$"/notes/{note.Id}" (note.Title)
│   ├── div class="col-lg-4 mb-3" // Sekcja Planów
│   │   ├── div class="card h-100"
│   │   │   ├── div class="card-header"
│   │   │   │   └── h5 ("Ostatnie Plany")
│   │   │   └── div class="card-body"
│   │   │       └── RecentPlansSection (Plans="RecentAcceptedPlans")
│   │   │           └── [if plans empty] EmptyState (Message="Brak zaakceptowanych planów")
│   │   │           └── [else] ul class="list-group list-group-flush"
│   │   │               └── foreach plan in Plans
│   │   │                   └── li class="list-group-item"
│   │   │                       └── NavLink href=$"/plans/{plan.Id}" (plan.Title)
│   ├── div class="col-lg-4 mb-3" // Sekcja Propozycji
│   │   ├── div class="card h-100"
│   │   │   ├── div class="card-header"
│   │   │   │   └── h5 ("Oczekujące Propozycje")
│   │   │   └── div class="card-body"
│   │   │       └── PendingProposalsSection (Count="PendingProposalCount")
│   │   │           └── [if count == 0] p ("Brak oczekujących propozycji.")
│   │   │           └── [else] NavLink href="/plan-proposals" ($"{Count} oczekujących propozycji")
└── QuickNoteModal (@bind-Visible="ShowQuickNoteModal") // Może być umieszczony w MainLayout dla globalnego dostępu, ale sterowany z Home
```

## 4. Szczegóły komponentów

### `Home.razor` (Komponent strony)
- **Opis komponentu:** Główny kontener widoku Dashboard. Odpowiedzialny za pobieranie danych, zarządzanie stanem ładowania/błędów i renderowanie sekcji z notatkami, planami i propozycjami. Kontroluje widoczność modala `QuickNoteModal`.
- **Główne elementy:** Kontener `div` z Bootstrap `row` i `col-lg-4` dla sekcji. Wykorzystuje komponenty `LoadingSpinner`, `RecentNotesSection`, `RecentPlansSection`, `PendingProposalsSection`, `EmptyState` oraz przycisk do otwierania `QuickNoteModal`. Wyświetla `ErrorMessage` w razie błędu.
- **Obsługiwane interakcje:**
    - `OnInitializedAsync`: Pobiera dane (notatki, plany, liczbę propozycji) z serwisów.
    - Kliknięcie przycisku "+ Nowa": Ustawia `ShowQuickNoteModal = true`.
- **Obsługiwana walidacja:** Wymaga autoryzacji (`@attribute [Authorize]`).
- **Typy:** `List<NoteListItemDto>`, `List<PlanListItemDto>`, `int`, `bool`, `string?`.
- **Propsy:** Brak (jest to komponent strony).

### `RecentNotesSection.razor`
- **Opis komponentu:** Wyświetla listę linków do 3 ostatnich notatek lub komunikat `EmptyState`.
- **Główne elementy:** `ul` (`list-group`) z elementami `li` (`list-group-item`) zawierającymi `NavLink` do szczegółów notatki. Warunkowo renderuje `EmptyState`.
- **Obsługiwane interakcje:** Kliknięcie na `NavLink` (obsługiwane przez Blazor Router).
- **Obsługiwana walidacja:** Brak.
- **Typy:** `List<NoteListItemDto>`.
- **Propsy:**
    - `[Parameter] public List<NoteListItemDto> Notes { get; set; }`

### `RecentPlansSection.razor`
- **Opis komponentu:** Wyświetla listę linków do 3 ostatnich zaakceptowanych planów lub komunikat `EmptyState`.
- **Główne elementy:** `ul` (`list-group`) z elementami `li` (`list-group-item`) zawierającymi `NavLink` do szczegółów planu. Warunkowo renderuje `EmptyState`.
- **Obsługiwane interakcje:** Kliknięcie na `NavLink` (obsługiwane przez Blazor Router).
- **Obsługiwana walidacja:** Brak.
- **Typy:** `List<PlanListItemDto>`.
- **Propsy:**
    - `[Parameter] public List<PlanListItemDto> Plans { get; set; }`

### `PendingProposalsSection.razor`
- **Opis komponentu:** Wyświetla liczbę oczekujących propozycji planów jako link do widoku propozycji lub komunikat o ich braku.
- **Główne elementy:** `NavLink` do `/plan-proposals` wyświetlający liczbę propozycji lub element `p` z informacją o braku propozycji.
- **Obsługiwane interakcje:** Kliknięcie na `NavLink` (obsługiwane przez Blazor Router).
- **Obsługiwana walidacja:** Brak.
- **Typy:** `int`.
- **Propsy:**
    - `[Parameter] public int Count { get; set; }`

### `QuickNoteModal.razor` (Komponent współdzielony - użycie)
- **Opis komponentu:** Modal do szybkiego tworzenia notatki. Jego implementacja jest poza zakresem tego planu, ale Dashboard musi kontrolować jego widoczność.
- **Główne elementy:** Formularz z polami na tytuł i treść, przyciski Zapisz/Anuluj.
- **Obsługiwane interakcje:** Zapisanie notatki (wywołuje `INoteService.CreateNoteAsync`), Anulowanie. Zamknięcie modala powinno ustawić `ShowQuickNoteModal = false` w `Home.razor` (poprzez `@bind-Visible`).
- **Obsługiwana walidacja:** Wewnętrzna walidacja formularza (np. wymagany tytuł).
- **Typy:** Wewnętrzne (np. model dla formularza), `bool` (dla widoczności).
- **Propsy (dla kontroli widoczności):**
    - `[Parameter] public bool Visible { get; set; }`
    - `[Parameter] public EventCallback<bool> VisibleChanged { get; set; }` (dla wsparcia `@bind-Visible`)

### `LoadingSpinner.razor` (Komponent współdzielony - użycie)
- **Opis komponentu:** Wyświetla animację ładowania.
- **Propsy:** Przyjmuje `bool` wskazujący, czy ma być widoczny.

### `EmptyState.razor` (Komponent współdzielony - użycie)
- **Opis komponentu:** Wyświetla komunikat, gdy lista jest pusta.
- **Propsy:**
    - `[Parameter] public string Message { get; set; }`
    - `[Parameter] public RenderFragment? ChildContent { get; set; }` (opcjonalnie, na dodatkowe akcje/przyciski)

## 5. Typy
Widok `Home.razor` będzie korzystał z istniejących DTO zdefiniowanych w warstwie usług i DTOs:

- **`_10xVibeTravels.Dtos.NoteListItemDto`**:
    - `Id` (Guid): Identyfikator notatki.
    - `Title` (string): Tytuł notatki (wyświetlany i używany w linku).
    - `ContentPreview` (string): Podgląd treści (nieużywany bezpośrednio na Dashboard).
    - `CreatedAt` (DateTime): Data utworzenia (nieużywana bezpośrednio na Dashboard).
    - `ModifiedAt` (DateTime): Data modyfikacji (używana do sortowania w API).
- **`_10xVibeTravels.Dtos.PlanListItemDto`**:
    - `Id` (Guid): Identyfikator planu.
    - `Status` (PlanStatus enum): Status planu (filtrowany w API).
    - `Title` (string): Tytuł planu (wyświetlany i używany w linku).
    *   `ContentPreview` (string) - Nieużywany bezpośrednio na Dashboard.
    *   `StartDate` (DateOnly) - Nieużywana bezpośrednio na Dashboard.
    *   `EndDate` (DateOnly) - Nieużywana bezpośrednio na Dashboard.
    *   `Budget` (decimal?) - Nieużywany bezpośrednio na Dashboard.
    *   `CreatedAt` (DateTime) - Nieużywana bezpośrednio na Dashboard.
    *   `ModifiedAt` (DateTime) - Używana do sortowania w API.
- **`_10xVibeTravels.Data.PlanStatus` (Enum)**: Używany w logice pobierania danych (mapowany z/na string w komunikacji API/DB).
- **`_10xVibeTravels.Requests.GetNotesListQuery`**: Używany do zapytania API `GET /notes`.
    - `Page` (int): Numer strony (ustawiony na 1).
    - `PageSize` (int): Liczba elementów (ustawiona na 3).
    - `SortBy` (string): Pole sortowania (ustawione na "modifiedAt").
    - `SortDirection` (string): Kierunek sortowania (ustawiony na "desc").
- **`_10xVibeTravels.Requests.PlanListQueryParameters`**: Używany do zapytania API `GET /plans`.
    - `Status` (string): Filtr statusu ("Accepted" lub "Generated").
    - `Page` (int): Numer strony (ustawiony na 1).
    - `PageSize` (int): Liczba elementów (ustawiona na 3 dla Accepted, 1 dla Generated).
    - `SortBy` (string): Pole sortowania (ustawione na "modifiedAt").
    - `SortDirection` (string): Kierunek sortowania (ustawiony na "desc").

Nie przewiduje się potrzeby tworzenia nowych, niestandardowych typów ViewModel dla tego widoku.

## 6. Zarządzanie stanem
Stan widoku `Home.razor` będzie zarządzany bezpośrednio w komponencie za pomocą zmiennych prywatnych:
- `bool IsLoading`: Flaga wskazująca proces ładowania danych (kontrola `LoadingSpinner`). Inicjalizowana na `true`.
- `List<NoteListItemDto>? RecentNotes`: Lista ostatnich notatek. Inicjalizowana na `null`.
- `List<PlanListItemDto>? RecentAcceptedPlans`: Lista ostatnich zaakceptowanych planów. Inicjalizowana na `null`.
- `int PendingProposalCount`: Liczba oczekujących propozycji. Inicjalizowana na `0`.
- `string? ErrorMessage`: Komunikat błędu w przypadku problemów z pobieraniem danych. Inicjalizowany na `null`.
- `bool ShowQuickNoteModal`: Flaga kontrolująca widoczność modala `QuickNoteModal`. Inicjalizowana na `false`.

Do pobierania danych zostaną wstrzyknięte serwisy (`INoteService`, `IPlanService`) o zasięgu `Scoped`. Aktualizacje stanu będą wyzwalane asynchronicznie w metodzie `OnInitializedAsync`, a Blazor automatycznie odświeży UI po zakończeniu operacji `await`. Nie ma potrzeby stosowania niestandardowych hooków ani bardziej złożonych wzorców zarządzania stanem dla tego widoku.

## 7. Integracja API
Integracja z API będzie realizowana poprzez wstrzyknięte serwisy `INoteService` i `IPlanService`.

W metodzie `OnInitializedAsync` komponentu `Home.razor` zostaną wykonane następujące wywołania:

1.  **Pobranie ostatnich notatek:**
    - Wywołanie: `noteService.GetNotesAsync(userId, new GetNotesListQuery { Page = 1, PageSize = 3, SortBy = "modifiedAt", SortDirection = "desc" })`
    - `userId` zostanie pobrany z `HttpContextAccessor` lub kontekstu autoryzacji.
    - Oczekiwana odpowiedź: `PaginatedListDto<NoteListItemDto>`
    - Akcja: Zapisanie `result.Items` do stanu `RecentNotes`.
2.  **Pobranie ostatnich zaakceptowanych planów:**
    - Wywołanie: `planService.GetPlansAsync(userId, new PlanListQueryParameters { Status = "Accepted", Page = 1, PageSize = 3, SortBy = "modifiedAt", SortDirection = "desc" })`
    - Oczekiwana odpowiedź: `PaginatedResult<PlanListItemDto>`
    - Akcja: Zapisanie `result.Items` do stanu `RecentAcceptedPlans`.
3.  **Pobranie liczby oczekujących propozycji:**
    - Wywołanie: `planService.GetPlansAsync(userId, new PlanListQueryParameters { Status = "Generated", Page = 1, PageSize = 1 })`
    - Oczekiwana odpowiedź: `PaginatedResult<PlanListItemDto>`
    - Akcja: Zapisanie `result.TotalItems` do stanu `PendingProposalCount`.

Wywołania API zostaną wykonane współbieżnie za pomocą `Task.WhenAll` dla optymalizacji czasu ładowania.

## 8. Interakcje użytkownika
- **Ładowanie widoku:** Po przejściu na `/`, użytkownik widzi `LoadingSpinner`. Po zakończeniu pobierania danych, spinner znika, a sekcje (notatki, plany, propozycje) są wypełniane danymi lub komunikatami `EmptyState`. Przycisk "+ Nowa" jest widoczny.
- **Kliknięcie tytułu notatki:** Użytkownik jest przenoszony do `/notes/{id}`.
- **Kliknięcie tytułu planu:** Użytkownik jest przenoszony do `/plans/{id}`.
- **Kliknięcie linku liczby propozycji:** Użytkownik jest przenoszony do `/plan-proposals`.
- **Kliknięcie przycisku "+ Nowa":** Modal `QuickNoteModal` staje się widoczny.
- **Interakcja z `QuickNoteModal`:** Zamknięcie modala (przez zapis lub anulowanie) ukrywa go. Komponent `Home.razor` nie odświeża automatycznie danych po zamknięciu modala.

## 9. Warunki i walidacja
- **Autoryzacja:** Widok jest dostępny tylko dla zalogowanych użytkowników (`@attribute [Authorize]`). Blazor powinien automatycznie przekierować niezalogowanych użytkowników na stronę logowania.
- **Pobieranie danych:** Dane są pobierane tylko dla aktualnie zalogowanego użytkownika (obsługiwane po stronie API/serwisów przez filtrowanie `userId`).
- **Wyświetlanie sekcji:** Sekcje notatek i planów wyświetlają listy tylko jeśli odpowiednie dane zostały pobrane i nie są puste. W przeciwnym razie wyświetlany jest komponent `EmptyState`. Sekcja propozycji wyświetla link tylko jeśli `PendingProposalCount > 0`.
- **Modal `QuickNoteModal`:** Walidacja pól formularza (np. wymagany tytuł) jest obsługiwana wewnątrz tego komponentu.

## 10. Obsługa błędów
- **Błędy pobierania danych API:**
    - W `OnInitializedAsync`, wszystkie wywołania API powinny być objęte blokiem `try...catch`.
    - W przypadku wyjątku (np. błąd sieci, błąd serwera 500, błąd 4xx inny niż 401/404):
        - Złap wyjątek.
        - Ustaw `IsLoading = false`.
        - Zapisz ogólny komunikat błędu w stanie `ErrorMessage` (np. "Nie udało się załadować danych panelu. Spróbuj ponownie później.").
        - Wyświetl `ErrorMessage` użytkownikowi zamiast sekcji danych.
        - Zaloguj szczegóły wyjątku po stronie serwera dla celów diagnostycznych.
- **Puste dane:** Jeśli API zwróci puste listy (`Items`), nie jest to traktowane jako błąd. Komponenty `RecentNotesSection` i `RecentPlansSection` powinny renderować `EmptyState`. Jeśli `PendingProposalCount` wynosi 0, `PendingProposalsSection` wyświetli odpowiedni komunikat.
- **Błędy modala `QuickNoteModal`:** Błędy związane z zapisem notatki (np. walidacja, błąd API) powinny być obsługiwane i komunikowane użytkownikowi wewnątrz samego modala (np. komunikaty walidacji, powiadomienia toast). Dashboard nie reaguje bezpośrednio na te błędy.

## 11. Kroki implementacji
1.  **Utworzenie pliku komponentu:** Stwórz plik `Pages/Home.razor`.
2.  **Dodanie routingu i autoryzacji:** Dodaj `@page "/"` i `@attribute [Authorize]` na początku pliku.
3.  **Wstrzyknięcie zależności:** Wstrzyknij `INoteService`, `IPlanService` oraz `NavigationManager` (dla linków) i potencjalnie `IHttpContextAccessor` (jeśli `userId` nie jest łatwo dostępny inaczej) w bloku `@code`.
4.  **Definicja stanu:** Zdefiniuj prywatne zmienne stanu: `IsLoading`, `RecentNotes`, `RecentAcceptedPlans`, `PendingProposalCount`, `ErrorMessage`, `ShowQuickNoteModal`.
5.  **Implementacja `OnInitializedAsync`:**
    - Ustaw `IsLoading = true`.
    - W bloku `try`:
        - Pobierz `userId`.
        - Przygotuj zadania dla `noteService.GetNotesAsync`, `planService.GetPlansAsync` (dla 'Accepted') i `planService.GetPlansAsync` (dla 'Generated').
        - Użyj `await Task.WhenAll(...)` do współbieżnego wykonania zadań.
        - Przetwórz wyniki: przypisz `Items` do `RecentNotes`, `RecentAcceptedPlans` i `TotalItems` do `PendingProposalCount`.
    - W bloku `catch`:
        - Zaloguj błąd.
        - Ustaw `ErrorMessage`.
    - W bloku `finally`:
        - Ustaw `IsLoading = false`.
        - Wywołaj `StateHasChanged()` (choć zazwyczaj nie jest to konieczne po `await` w metodach cyklu życia).
6.  **Struktura HTML (Markup):**
    - Dodaj główną strukturę HTML z Bootstrap (`row`, `col-lg-4`, `card`).
    - Dodaj warunkowe renderowanie `LoadingSpinner` na podstawie `IsLoading`.
    - Dodaj warunkowe renderowanie `ErrorMessage` jeśli nie jest pusty.
    - Dodaj przycisk "+ Nowa" z `@onclick` ustawiającym `ShowQuickNoteModal = true`.
    - Dla każdej sekcji (Notatki, Plany, Propozycje):
        - Dodaj nagłówek (`h5` w `card-header`).
        - W `card-body`, dodaj logikę warunkową:
            - Jeśli dane są puste (po załadowaniu): Renderuj komponent `EmptyState` z odpowiednim komunikatem.
            - Jeśli dane istnieją: Renderuj listę (`ul`/`li`/`NavLink`) lub link (`NavLink` dla propozycji). Użyj `note.Id`, `plan.Id` do generowania linków.
7.  **Integracja `QuickNoteModal`:** Dodaj komponent `<QuickNoteModal @bind-Visible="ShowQuickNoteModal" />` w markupie (np. na końcu).
8.  **Utworzenie komponentów pomocniczych (jeśli nie istnieją):**
    - `RecentNotesSection.razor`: Utwórz prosty komponent przyjmujący `List<NoteListItemDto>` jako parametr i renderujący listę `NavLink` lub `EmptyState`.
    - `RecentPlansSection.razor`: Analogicznie dla `List<PlanListItemDto>`.
    - `PendingProposalsSection.razor`: Utwórz prosty komponent przyjmujący `int` jako parametr i renderujący `NavLink` lub tekst.
    - Upewnij się, że komponenty `LoadingSpinner` i `EmptyState` istnieją i są dostępne.
9.  **Styling (CSS):** Wykorzystaj klasy Bootstrap. Dodaj niestandardowe style tylko w razie potrzeby. Upewnij się, że karty mają równą wysokość w wierszu (`h-100` na `div.card`).
10. **Testowanie:** Przetestuj różne scenariusze:
    - Ładowanie strony.
    - Stan ładowania.
    - Wyświetlanie danych (notatki, plany, propozycje).
    - Puste stany dla każdej sekcji.
    - Obsługa błędów API.
    - Działanie linków do szczegółów.
    - Działanie linku do propozycji.
    - Otwieranie i zamykanie `QuickNoteModal`.
    - Responsywność na różnych rozmiarach ekranu. 