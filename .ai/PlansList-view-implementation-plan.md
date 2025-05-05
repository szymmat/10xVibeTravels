# Plan implementacji widoku PlansList

## 1. Przegląd
Widok `PlansList` służy do wyświetlania listy zapisanych (zaakceptowanych lub odrzuconych) planów podróży dla zalogowanego użytkownika. Umożliwia filtrowanie planów według statusu, sortowanie według różnych kryteriów (tytuł, data utworzenia, data modyfikacji), paginację wyników oraz usuwanie planów po potwierdzeniu. Widok integruje się z API w celu pobierania i usuwania danych.

## 2. Routing widoku
Widok powinien być dostępny pod ścieżką `/plans`. Wymaga autoryzacji użytkownika.
```csharp
// PlansList.razor
@page "/plans"
@attribute [Authorize] 
```

## 3. Struktura komponentów
```
PlansList.razor (@page "/plans", @attribute [Authorize])
├── LoadingSpinner (@if IsLoading)
├── ErrorDisplay (@if HasError)
├── @if !IsLoading && !HasError
│   ├── div.row (Filtering/Controls)
│   │   └── div.col (Filter Dropdown) 
│   │       └── Select (@bind="CurrentStatusFilter", @onchange="HandleFilterChange")
│   │           └── option (value="Accepted") "Zaakceptowane"
│   │           └── option (value="Rejected") "Odrzucone"
│   ├── @if PaginatedPlans != null && PaginatedPlans.Items.Any()
│   │   └── table.table.table-hover (Plan List)
│   │       ├── thead
│   │       │   └── tr
│   │       │       ├── SortableHeader (ColumnKey="title", Text="Tytuł", ...)
│   │       │       ├── th "Status"
│   │       │       ├── SortableHeader (ColumnKey="modifiedAt", Text="Data modyfikacji", ...)
│   │       │       ├── SortableHeader (ColumnKey="createdAt", Text="Data utworzenia", ...)
│   │       │       └── th "Akcje"
│   │       └── tbody
│   │           └── @foreach plan in PaginatedPlans.Items
│   │               └── tr
│   │                   ├── td ->NavLink (href="/plans/{plan.Id}") [@plan.Title]
│   │                   ├── td -> PlanStatusBadge (Status=plan.Status)
│   │                   ├── td [@plan.ModifiedAt.ToString("yyyy-MM-dd HH:mm")]
│   │                   ├── td [@plan.CreatedAt.ToString("yyyy-MM-dd HH:mm")]
│   │                   └── td -> button.btn.btn-sm.btn-danger (@onclick=@(() => ShowDeleteModal(plan))) [Ikona Kosza]
│   ├── @else (No plans found)
│   │   └── EmptyState (Message="Brak planów spełniających kryteria.")
│   └── PaginationControls (CurrentPage, TotalPages, @onchange="HandlePageChange")
└── DeleteConfirmationModal (IsVisible, PlanTitle=@PlanToDelete?.Title, @onconfirm="ConfirmDelete", @oncancel="CancelDelete") 
```
*(Uwaga: `SortableHeader`, `PlanStatusBadge`, `PaginationControls`, `DeleteConfirmationModal`, `LoadingSpinner`, `EmptyState`, `ErrorDisplay` mogą być komponentami współdzielonymi lub zaimplementowane w ramach `PlansList.razor`)*

## 4. Szczegóły komponentów

### `PlansList.razor` (Komponent strony)
*   **Opis:** Główny komponent widoku listy planów. Zarządza stanem (ładowanie, błędy, dane, filtry, sortowanie, paginacja, usuwanie), pobiera dane z API i renderuje interfejs użytkownika, w tym komponenty podrzędne lub ich odpowiedniki.
*   **Główne elementy:** Kontener widoku, logika w `@code`, warunkowe renderowanie (`LoadingSpinner`, `ErrorDisplay`, `EmptyState`, tabela planów), kontrolka filtrowania (`select`), tabela (`table`), kontrolki paginacji, modal potwierdzenia usunięcia.
*   **Obsługiwane interakcje:** Zmiana filtra statusu, zmiana sortowania (kliknięcie nagłówka), zmiana strony paginacji, zainicjowanie usunięcia planu (kliknięcie ikony kosza), potwierdzenie/anulowanie usunięcia w modalu.
*   **Obsługiwana walidacja:** Brak walidacji po stronie frontendu poza zapewnieniem poprawnych typów danych przesyłanych do API (obsługiwane przez logikę komponentu).
*   **Typy:** `PaginatedResult<PlanListItemDto>`, `PlanListItemDto`, `PlanStatus`. Zmienne stanu do zarządzania filtrowaniem, sortowaniem, paginacją, ładowaniem, błędami, stanem modala usuwania.
*   **Propsy:** Brak (jest to komponent strony). Wstrzykiwane zależności: `HttpClient` (lub dedykowany serwis API), `NavigationManager`.

### `SortableHeader` (Komponent pomocniczy lub implementacja w `PlansList`)
*   **Opis:** Nagłówek kolumny tabeli (`<th>`) umożliwiający sortowanie po kliknięciu. Wskazuje aktualny kierunek sortowania.
*   **Główne elementy:** `<th>` z obsługą `@onclick`, tekst nagłówka, ikona sortowania (np. FontAwesome `fa-sort`, `fa-sort-up`, `fa-sort-down`).
*   **Obsługiwane interakcje:** Kliknięcie (`@onclick`).
*   **Obsługiwana walidacja:** Brak.
*   **Typy:** `string` (klucz kolumny), `string` (tekst do wyświetlenia), `string` (aktualny klucz sortowania), `string` (aktualny kierunek sortowania).
*   **Propsy:** `ColumnKey`, `Text`, `CurrentSortKey`, `CurrentSortDirection`, `EventCallback<string> OnSort`.

### `PlanStatusBadge` (Komponent pomocniczy lub metoda w `PlansList`)
*   **Opis:** Wyświetla status planu jako kolorową etykietę Bootstrap (`badge`).
*   **Główne elementy:** `<span>` z odpowiednimi klasami CSS Bootstrap (`badge`, `bg-success`, `bg-danger` itp.).
*   **Obsługiwane interakcje:** Brak.
*   **Obsługiwana walidacja:** Brak.
*   **Typy:** `PlanStatus`.
*   **Propsy:** `PlanStatus Status`.

### `PaginationControls` (Komponent współdzielony)
*   **Opis:** Renderuje przyciski lub linki do nawigacji między stronami paginowanej listy. Wyświetla informacje o bieżącej stronie i całkowitej liczbie stron.
*   **Główne elementy:** Przyciski (`<button>`) lub linki (`<a>`) dla "Poprzednia", "Następna", numery stron. Elementy `<span>` do wyświetlania informacji "Strona X z Y".
*   **Obsługiwane interakcje:** Kliknięcie przycisków/linków nawigacyjnych (`@onclick`).
*   **Obsługiwana walidacja:** Wyłącza przycisk "Poprzednia" na pierwszej stronie i "Następna" na ostatniej stronie.
*   **Typy:** `int` (CurrentPage), `int` (TotalPages).
*   **Propsy:** `CurrentPage`, `TotalPages`, `EventCallback<int> OnPageChanged`.

### `DeleteConfirmationModal` (Komponent współdzielony)
*   **Opis:** Modal Bootstrapa wyświetlający pytanie o potwierdzenie usunięcia elementu (planu).
*   **Główne elementy:** Struktura modala Bootstrapa (`modal`, `modal-dialog`, `modal-content`, `modal-header`, `modal-body`, `modal-footer`), przyciski "Potwierdź" i "Anuluj".
*   **Obsługiwane interakcje:** Kliknięcie przycisków "Potwierdź" (`@onclick`) i "Anuluj" (`@onclick`), zamknięcie modala.
*   **Obsługiwana walidacja:** Brak.
*   **Typy:** `bool` (IsVisible), `string?` (tytuł planu do wyświetlenia w komunikacie).
*   **Propsy:** `IsVisible`, `PlanTitle`, `EventCallback OnConfirm`, `EventCallback OnCancel`.

## 5. Typy

*   **`_10xVibeTravels.Dtos.PaginatedResult<PlanListItemDto>`:**
    *   `Items` (`List<PlanListItemDto>`): Lista planów na bieżącej stronie.
    *   `Page` (`int`): Numer bieżącej strony.
    *   `PageSize` (`int`): Liczba elementów na stronie.
    *   `TotalItems` (`int`): Całkowita liczba planów pasujących do kryteriów.
    *   `TotalPages` (`int`): Całkowita liczba stron.
*   **`_10xVibeTravels.Dtos.PlanListItemDto`:**
    *   `Id` (`Guid`): Identyfikator planu.
    *   `Status` (`_10xVibeTravels.Data.PlanStatus`): Status planu (enum: `Generated`, `Accepted`, `Rejected`).
    *   `Title` (`string`): Tytuł planu.
    *   `ContentPreview` (`string`): Podgląd treści (pierwsze 150 znaków). (Może nie być używany bezpośrednio w tabeli).
    *   `StartDate` (`DateOnly`): Data rozpoczęcia podróży. (Opcjonalne w tabeli).
    *   `EndDate` (`DateOnly`): Data zakończenia podróży. (Opcjonalne w tabeli).
    *   `Budget` (`decimal`): Budżet planu. (Opcjonalne w tabeli).
    *   `CreatedAt` (`DateTime`): Data utworzenia planu.
    *   `ModifiedAt` (`DateTime`): Data ostatniej modyfikacji planu.
*   **`_10xVibeTravels.Data.PlanStatus` (Enum):**
    *   `Generated`
    *   `Accepted`
    *   `Rejected`
*   **Typy Pomocnicze/Mapowania (w `PlansList.razor`):**
    *   `Dictionary<PlanStatus, string> StatusDisplayNames`: Mapowanie enum -> polska nazwa (np. `Accepted` -> "Zaakceptowane").
    *   `Dictionary<PlanStatus, string> StatusBadgeClasses`: Mapowanie enum -> klasa CSS badge (np. `Accepted` -> "badge bg-success").
    *   `Dictionary<string, string> StatusFilterOptions`: Opcje dla dropdown filtra (np. `"Accepted"` -> "Zaakceptowane").

## 6. Zarządzanie stanem
Stan widoku będzie zarządzany wewnątrz komponentu `PlansList.razor` przy użyciu standardowych mechanizmów Blazor Server.
*   **Zmienne stanu:**
    *   `private PaginatedResult<PlanListItemDto>? paginatedPlans;`
    *   `private bool isLoading = true;`
    *   `private string? errorMessage;`
    *   `private string currentStatusFilter = "Accepted";` // Wartość dla API
    *   `private string currentSortBy = "modifiedAt";`
    *   `private string currentSortDirection = "desc";`
    *   `private int currentPage = 1;`
    *   `private int pageSize = 10;`
    *   `private PlanListItemDto? planToDelete;`
    *   `private bool showDeleteModal;`
*   **Inicjalizacja:** Dane ładowane w `OnInitializedAsync`.
*   **Aktualizacje:** Zmiany filtrów, sortowania, paginacji lub usunięcie planu wywołują metodę `LoadPlansAsync`, która aktualizuje stan i odświeża interfejs. Stan modala (`showDeleteModal`, `planToDelete`) jest zarządzany lokalnie.
*   **Wstrzykiwanie Zależności:** `@inject HttpClient Http` (lub dedykowany serwis), `@inject NavigationManager NavigationManager`.

## 7. Integracja API

*   **Pobieranie Listy Planów:**
    *   **Metoda:** `LoadPlansAsync()`
    *   **Endpoint:** `GET /plans`
    *   **Parametry Zapytania:** `status`, `sortBy`, `sortDirection`, `page`, `pageSize` (budowane dynamicznie na podstawie zmiennych stanu).
    *   **Typ Żądania:** Brak (parametry w URL).
    *   **Typ Odpowiedzi:** `_10xVibeTravels.Dtos.PaginatedResult<PlanListItemDto>`
    *   **Obsługa:** Wywołanie `Http.GetFromJsonAsync<PaginatedResult<PlanListItemDto>>(url)`. Aktualizacja stanu `paginatedPlans`, `isLoading`, `errorMessage`.
*   **Usuwanie Planu:**
    *   **Metoda:** `ConfirmDelete()`
    *   **Endpoint:** `DELETE /plans/{id}`
    *   **Parametry Zapytania:** Brak. `id` w ścieżce URL (`planToDelete.Id`).
    *   **Typ Żądania:** Brak.
    *   **Typ Odpowiedzi:** `204 No Content`.
    *   **Obsługa:** Wywołanie `Http.DeleteAsync(url)`. Sprawdzenie statusu odpowiedzi. Po sukcesie: wywołanie `LoadPlansAsync()` w celu odświeżenia listy, zresetowanie `planToDelete` i `showDeleteModal`.

## 8. Interakcje użytkownika

*   **Wejście na stronę:** Wyświetlenie `LoadingSpinner`, wywołanie `LoadPlansAsync` z domyślnymi parametrami (status="Accepted", sort="modifiedAt desc", page=1). Po załadowaniu: wyświetlenie tabeli planów lub `EmptyState`.
*   **Zmiana Filtra:** Użytkownik wybiera opcję w dropdownie -> aktualizacja `currentStatusFilter` -> wywołanie `LoadPlansAsync`.
*   **Sortowanie:** Użytkownik klika nagłówek kolumny sortowalnej -> aktualizacja `currentSortBy` i `currentSortDirection` (przełączenie kierunku, jeśli ta sama kolumna) -> wywołanie `LoadPlansAsync`.
*   **Paginacja:** Użytkownik klika przycisk zmiany strony -> aktualizacja `currentPage` -> wywołanie `LoadPlansAsync`.
*   **Kliknięcie Tytułu Planu:** Użytkownik klika link tytułu -> nawigacja do `/plans/{plan.Id}` za pomocą `NavigationManager.NavigateTo()`.
*   **Inicjacja Usunięcia:** Użytkownik klika ikonę kosza -> wywołanie `ShowDeleteModal(plan)` -> ustawienie `planToDelete` i `showDeleteModal = true` -> pokazanie modala.
*   **Potwierdzenie Usunięcia:** Użytkownik klika "Potwierdź" w modalu -> wywołanie `ConfirmDelete()` -> wywołanie API `DELETE` -> ukrycie modala -> wywołanie `LoadPlansAsync`.
*   **Anulowanie Usunięcia:** Użytkownik klika "Anuluj" lub zamyka modal -> wywołanie `CancelDelete()` -> ukrycie modala, zresetowanie `planToDelete`.

## 9. Warunki i walidacja
*   **Autoryzacja:** Widok jest chroniony atrybutem `[Authorize]`.
*   **Parametry API:** Komponent zapewnia, że wartości przekazywane do API (`status`, `sortBy`, `sortDirection`, `page`, `pageSize`) są zgodne z oczekiwaniami API (np. poprawne stringi dla statusu i sortowania, dodatnie liczby całkowite dla paginacji). Logika komponentu kontroluje te wartości.
*   **Interfejs:** Przyciski paginacji "Poprzednia"/"Następna" są wyłączane (`disabled`), gdy użytkownik jest odpowiednio na pierwszej/ostatniej stronie.

## 10. Obsługa błędów
*   **Błąd ładowania planów:** W bloku `catch` w `LoadPlansAsync`: ustawienie `isLoading = false`, zapisanie komunikatu błędu w `errorMessage`, wyczyszczenie `paginatedPlans`. Wyświetlenie `ErrorDisplay` z przyjaznym komunikatem (np. "Wystąpił błąd podczas ładowania planów.").
*   **Błąd usuwania planu:** W bloku `catch` w `ConfirmDelete`: ustawienie `isLoading = false` (jeśli był ustawiony wskaźnik usuwania), zapisanie komunikatu błędu w `errorMessage`. Wyświetlenie błędu (np. za pomocą tostera lub komunikatu w okolicy modala). Nie odświeżanie listy.
*   **Nieautoryzowany dostęp / Brak uprawnień (401/403):** Błędy te powinny być przechwytywane globalnie lub w dedykowanym serwisie API. Mogą prowadzić do przekierowania na stronę logowania lub wyświetlenia ogólnego komunikatu błędu.
*   **Plan nie znaleziony (404):** Może wystąpić przy próbie usunięcia. Wyświetlić komunikat błędu informujący, że plan nie istnieje lub został już usunięty.

## 11. Kroki implementacji
1.  **Utworzenie pliku komponentu:** Stwórz plik `PlansList.razor` w odpowiednim folderze (np. `Pages/Plans/` lub `Features/Plans/`).
2.  **Dodanie routingu i autoryzacji:** Dodaj dyrektywy `@page "/plans"` i `@attribute [Authorize]`.
3.  **Wstrzyknięcie zależności:** Dodaj `@inject HttpClient Http` i `@inject NavigationManager NavigationManager`.
4.  **Zdefiniowanie zmiennych stanu:** W bloku `@code` zadeklaruj wszystkie potrzebne zmienne (`isLoading`, `paginatedPlans`, `currentStatusFilter` itd.).
5.  **Implementacja `OnInitializedAsync`:** Wywołaj `LoadPlansAsync()` w tej metodzie.
6.  **Implementacja `LoadPlansAsync`:**
    *   Ustaw `isLoading = true`, `errorMessage = null`.
    *   Zbuduj URL zapytania `GET /plans` z aktualnymi parametrami stanu (filtr, sortowanie, paginacja).
    *   Wywołaj API używając `Http.GetFromJsonAsync`.
    *   W bloku `try...catch` obsłuż odpowiedź sukcesu (przypisz wynik do `paginatedPlans`) i błędu (ustaw `errorMessage`).
    *   Ustaw `isLoading = false` w bloku `finally`.
7.  **Implementacja struktury HTML:** Dodaj podstawowy układ strony, w tym warunkowe renderowanie dla `isLoading`, `errorMessage`.
8.  **Implementacja Filtra Statusu:** Dodaj `<select>` z opcjami "Zaakceptowane", "Odrzucone". Powiąż wartość z `currentStatusFilter` (`@bind`). Dodaj `@onchange` wywołujący metodę `HandleFilterChange`, która aktualizuje `currentPage = 1` i wywołuje `LoadPlansAsync`.
9.  **Implementacja Tabeli Planów:**
    *   Dodaj `<table>` z `<thead>` i `<tbody>`.
    *   W `<thead>` dodaj nagłówki (`<th>`). Dla kolumn sortowalnych zaimplementuj logikę sortowania (np. komponent `SortableHeader` lub bezpośrednio w `<th>`): `@onclick` wywołujący `HandleSortChange(columnKey)`, który aktualizuje `currentSortBy`, `currentSortDirection`, `currentPage = 1` i wywołuje `LoadPlansAsync`. Wyświetlaj ikony sortowania.
    *   W `<tbody>` użyj `@foreach` do iteracji po `paginatedPlans.Items`. Renderuj wiersze (`<tr>`) i komórki (`<td>`) z danymi planu.
    *   Sformatuj daty (`ToString("yyyy-MM-dd HH:mm")`).
    *   Użyj `NavLink` dla tytułu.
    *   Implementuj `PlanStatusBadge` (jako metodę lub komponent) do wyświetlania statusu.
    *   Dodaj przycisk usuwania z ikoną kosza, wywołujący `ShowDeleteModal(plan)` po kliknięciu.
10. **Implementacja `EmptyState`:** Dodaj warunek `@if (paginatedPlans == null || !paginatedPlans.Items.Any())` i renderuj komponent `EmptyState` lub prosty komunikat.
11. **Implementacja Paginacji:** Dodaj komponent `PaginationControls` (lub zaimplementuj logikę), przekazując `currentPage` i `totalPages` z `paginatedPlans`. Obsłuż zdarzenie zmiany strony (`OnPageChanged`), które wywołuje `HandlePageChange(newPage)`, aktualizuje `currentPage` i wywołuje `LoadPlansAsync`.
12. **Implementacja Modala Usuwania:**
    *   Dodaj komponent `DeleteConfirmationModal`. Powiąż jego widoczność z `showDeleteModal`. Przekaż `planToDelete?.Title`.
    *   Implementuj metody `ShowDeleteModal(plan)`, `CancelDelete()`, `ConfirmDelete()`.
    *   `ConfirmDelete` wywołuje `Http.DeleteAsync`, obsługuje odpowiedź i w przypadku sukcesu wywołuje `LoadPlansAsync`.
13. **Styling:** Użyj klas Bootstrapa do stylowania tabeli, przycisków, modala, badge'y itp.
14. **Testowanie:** Przetestuj wszystkie funkcjonalności: ładowanie, filtrowanie, sortowanie (różne kolumny, zmiana kierunku), paginację, usuwanie (potwierdzenie i anulowanie), obsługę błędów i stan pusty. 