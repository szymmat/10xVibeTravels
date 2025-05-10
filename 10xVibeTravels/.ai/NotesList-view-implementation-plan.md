# Plan implementacji widoku Lista Notatek (`NotesList`)

## 1. Przegląd
Widok `NotesList` ma na celu wyświetlenie paginowanej i sortowalnej listy notatek należących do zalogowanego użytkownika. Umożliwia szybkie przeglądanie tytułów i fragmentów treści notatek, sortowanie listy według różnych kryteriów oraz usuwanie niepotrzebnych notatek po potwierdzeniu. W przypadku braku notatek, widok wyświetla stosowny komunikat i zachęca do utworzenia pierwszej notatki.

## 2. Routing widoku
Widok powinien być dostępny pod ścieżką `/notes`. Należy użyć dyrektywy `@page "/notes"` w pliku `NotesList.razor`.

## 3. Struktura komponentów
Hierarchia komponentów dla widoku `NotesList`:

```
NotesList.razor (@page "/notes")
├── LoadingSpinner.razor (Warunkowo, podczas ładowania danych)
├── EmptyState.razor (Warunkowo, jeśli lista notatek jest pusta)
│   └── Przycisk "Utwórz pierwszą notatkę" (nawiguje do /notes/create)
├── Tabela HTML (`<table>`) (Warunkowo, jeśli istnieją notatki)
│   ├── Nagłówek tabeli (`<thead>`)
│   │   ├── SortableTableHeader.razor (Kolumna: Tytuł)
│   │   ├── SortableTableHeader.razor (Kolumna: Data utworzenia)
│   │   ├── SortableTableHeader.razor (Kolumna: Data modyfikacji)
│   │   └── Komórka nagłówka (`<th>`) (Kolumna: Akcje)
│   ├── Ciało tabeli (`<tbody>`)
│   │   └── Wiersz tabeli (`<tr>`) (Renderowany w pętli dla każdej notatki)
│   │       ├── Komórka (`<td>`) (Tytuł - potencjalnie link do /notes/{id})
│   │       ├── Komórka (`<td>`) (Data utworzenia)
│   │       ├── Komórka (`<td>`) (Data modyfikacji)
│   │       ├── Komórka (`<td>`)
│   │       │   └── Przycisk/Ikona "Usuń" (Otwiera DeleteConfirmationModal)
├── PaginationControls.razor (Warunkowo, jeśli jest więcej niż jedna strona)
└── DeleteConfirmationModal.razor (Warunkowo, widoczny po kliknięciu "Usuń")
    ├── Przycisk "Potwierdź" (Wywołuje akcję usunięcia)
    └── Przycisk "Anuluj" (Zamyka modal)

```

## 4. Szczegóły komponentów

### `NotesList.razor` (Komponent strony)
- **Opis komponentu:** Główny komponent strony, odpowiedzialny za pobieranie danych z API, zarządzanie stanem widoku (ładowanie, błędy, dane, sortowanie, paginacja, stan modala usuwania) oraz koordynację komponentów podrzędnych. Wykorzystuje `INoteService` do interakcji z API.
- **Główne elementy:** Kontener dla całego widoku, warunkowe renderowanie `LoadingSpinner`, `EmptyState`, tabeli z notatkami, `PaginationControls` oraz `DeleteConfirmationModal`. Wyświetla komunikaty o błędach.
- **Obsługiwane interakcje:**
    - Inicjalizacja ładowania danych przy wejściu na stronę (`OnInitializedAsync`).
    - Zmiana strony paginacji (wywołana przez `PaginationControls`).
    - Zmiana kryterium i kierunku sortowania (wywołana przez `SortableTableHeader`).
    - Otwarcie modala potwierdzenia usunięcia (wywołane kliknięciem ikony usuwania w wierszu tabeli).
    - Potwierdzenie usunięcia notatki (wywołane przez `DeleteConfirmationModal`).
    - Anulowanie usuwania notatki (wywołane przez `DeleteConfirmationModal`).
- **Obsługiwana walidacja:** Brak walidacji wprowadzanych danych przez użytkownika. Walidacja parametrów `page`, `pageSize`, `sortBy`, `sortDirection` odbywa się pośrednio przez logikę komponentu przed wywołaniem API (np. `page` >= 1).
- **Typy:** `NotesListViewState`, `PaginatedListDto<NoteListItemDto>`, `NoteListItemDto`, `INoteService`, `NavigationManager`.
- **Propsy:** Brak (jest to komponent strony).

### `SortableTableHeader.razor` (Komponent nagłówka tabeli)
- **Opis komponentu:** Komponent wielokrotnego użytku reprezentujący nagłówek kolumny tabeli, który umożliwia sortowanie. Wyświetla nazwę kolumny oraz ikonę wskazującą aktualny stan sortowania (rosnąco, malejąco, brak).
- **Główne elementy:** Element `<th>` z tekstem nagłówka i ikoną sortowania (np. Bootstrap Icons). Element jest klikalny.
- **Obsługiwane interakcje:** Kliknięcie na nagłówek cyklicznie zmienia stan sortowania (np. DESC -> ASC -> brak/domyślne -> DESC) i wywołuje zdarzenie zwrotne (`EventCallback`) z nowymi parametrami sortowania (`sortBy`, `sortDirection`).
- **Obsługiwana walidacja:** Brak.
- **Typy:** `string` (dla `sortBy` tej kolumny), `string` (aktualnie wybrany `sortBy`), `string` (aktualny `sortDirection`).
- **Propsy:**
    - `ColumnTitle` (string): Wyświetlana nazwa kolumny.
    - `SortIdentifier` (string): Identyfikator pola do sortowania dla tej kolumny (np. "title", "createdAt").
    - `CurrentSortBy` (string): Aktualnie aktywne pole sortowania w komponencie nadrzędnym.
    - `CurrentSortDirection` (string): Aktualnie aktywny kierunek sortowania w komponencie nadrzędnym.
    - `OnSortChanged` (EventCallback<(string SortBy, string SortDirection)>): Zdarzenie wywoływane po kliknięciu, przekazujące nowy stan sortowania.

### `PaginationControls.razor` (Komponent paginacji)
- **Opis komponentu:** Komponent wielokrotnego użytku do wyświetlania kontrolek paginacji (przyciski "Poprzednia", "Następna", opcjonalnie numery stron).
- **Główne elementy:** Przyciski (`<button>`) lub linki (`<a>`) dla nawigacji między stronami. Może wyświetlać informacje typu "Strona X z Y".
- **Obsługiwane interakcje:** Kliknięcie na przycisk "Poprzednia" lub "Następna" (lub numer strony) wywołuje zdarzenie zwrotne (`EventCallback<int>`) z nowym numerem strony. Przyciski są wyłączane (`disabled`), gdy osiągnięto pierwszą lub ostatnią stronę.
- **Obsługiwana walidacja:** Brak.
- **Typy:** `int` (dla `CurrentPage`, `TotalPages`).
- **Propsy:**
    - `CurrentPage` (int): Aktualny numer strony.
    - `TotalPages` (int): Całkowita liczba stron.
    - `OnPageChanged` (EventCallback<int>): Zdarzenie wywoływane po zmianie strony, przekazujące nowy numer strony.

### `DeleteConfirmationModal.razor` (Komponent modala potwierdzenia)
- **Opis komponentu:** Komponent wielokrotnego użytku wyświetlający modalne okno dialogowe z prośbą o potwierdzenie akcji usunięcia.
- **Główne elementy:** Kontener modala (np. Bootstrap Modal), tekst pytania (np. "Czy na pewno chcesz usunąć tę notatkę?"), przycisk "Potwierdź" i przycisk "Anuluj".
- **Obsługiwane interakcje:**
    - Kliknięcie "Potwierdź" wywołuje zdarzenie `OnConfirm` (`EventCallback`).
    - Kliknięcie "Anuluj" lub zamknięcie modala wywołuje zdarzenie `OnCancel` (`EventCallback`).
- **Obsługiwana walidacja:** Brak.
- **Typy:** Brak specyficznych typów.
- **Propsy:**
    - `IsVisible` (bool): Kontroluje widoczność modala.
    - `OnConfirm` (EventCallback): Zdarzenie wywoływane po kliknięciu "Potwierdź".
    - `OnCancel` (EventCallback): Zdarzenie wywoływane po kliknięciu "Anuluj" lub zamknięciu.

### `LoadingSpinner.razor` / `EmptyState.razor`
- **Opis:** Standardowe komponenty współdzielone do wskazywania ładowania i pustego stanu, zgodne z UI Plan. `EmptyState` zawiera przycisk nawigujący do `/notes/create`.
- **Propsy:** `EmptyState` może przyjmować tekst wiadomości i tekst przycisku jako parametry.

## 5. Typy
- **`NoteListItemDto`**: Obiekt transferu danych (DTO) reprezentujący pojedynczą notatkę na liście.
    ```csharp
    public record NoteListItemDto(
        Guid Id,
        string Title,
        string ContentPreview, // Pierwsze 150 znaków treści
        DateTime CreatedAt,
        DateTime ModifiedAt
    );
    ```
- **`PaginatedListDto<T>`**: Generyczny DTO dla paginowanych list. W tym widoku używany jako `PaginatedListDto<NoteListItemDto>`.
    ```csharp
    public record PaginatedListDto<T>(
        List<T> Items,
        int Page,
        int PageSize,
        int TotalItems,
        int TotalPages
    );
    ```
- **`NotesListViewState`** (Klasa pomocnicza w `@code` bloku `NotesList.razor`): Przechowuje cały stan widoku.
    ```csharp
    private class NotesListViewState
    {
        public bool IsLoading { get; set; } = true; // Domyślnie ładujemy dane na starcie
        public string? ErrorMessage { get; set; }
        public PaginatedListDto<NoteListItemDto>? NotesData { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10; // Zgodnie z PRD
        public string SortBy { get; set; } = "modifiedAt"; // Domyślne sortowanie wg API Plan
        public string SortDirection { get; set; } = "desc"; // Domyślne sortowanie wg API Plan
        public Guid? NoteToDeleteId { get; set; } // ID notatki do usunięcia
        public bool IsDeleteModalVisible { get; set; } = false; // Czy modal jest widoczny
    }
    ```
- **`GetNotesListQuery`** (Używany przez `INoteService.GetNotesAsync`): Obiekt przekazujący parametry zapytania do serwisu. Odpowiada parametrom Query z API Plan.
    ```csharp
    // Zakładamy istnienie takiego lub podobnego obiektu Request/Query
    public class GetNotesListQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = "modifiedAt";
        public string? SortDirection { get; set; } = "desc";
    }
    ```

## 6. Zarządzanie stanem
Stan widoku będzie zarządzany lokalnie w komponencie `NotesList.razor` przy użyciu prywatnej klasy `NotesListViewState` (lub bezpośrednio pól w bloku `@code`). Zależności, takie jak `INoteService` i `NavigationManager`, zostaną wstrzyknięte za pomocą dependency injection (`@inject`). Nie przewiduje się potrzeby tworzenia dedykowanych custom hooków (w kontekście Blazor - dodatkowych serwisów stanu) dla tego widoku. Stan będzie aktualizowany w odpowiedzi na interakcje użytkownika (zmiana strony, sortowanie, usuwanie) oraz po zakończeniu wywołań API.

## 7. Integracja API
Integracja z backendem odbywa się poprzez wstrzyknięty serwis `INoteService`.

- **Pobieranie listy notatek:**
    - Metoda serwisowa: `INoteService.GetNotesAsync(string userId, GetNotesListQuery queryParams)` (zakładając, że `userId` jest pobierany wewnętrznie przez serwis lub przekazywany). Frontend wywołuje ją przekazując obiekt `GetNotesListQuery` zbudowany na podstawie `NotesListViewState` (`CurrentPage`, `PageSize`, `SortBy`, `SortDirection`).
    - Endpoint: `GET /notes`
    - Typy:
        - Żądanie (parametry Query): `page` (int), `pageSize` (int), `sortBy` (string), `sortDirection` (string). Odzwierciedlone w `GetNotesListQuery`.
        - Odpowiedź (sukces): `PaginatedListDto<NoteListItemDto>`
        - Odpowiedź (błąd): `400 Bad Request`, `401 Unauthorized`, `500 Internal Server Error`.
- **Usuwanie notatki:**
    - Metoda serwisowa: `INoteService.DeleteNoteAsync(string userId, Guid noteId)` (zakładając, że `userId` jest pobierany wewnętrznie). Frontend wywołuje ją przekazując `NotesListViewState.NoteToDeleteId`.
    - Endpoint: `DELETE /notes/{id}`
    - Typy:
        - Żądanie (parametr Path): `id` (guid).
        - Odpowiedź (sukces): `204 No Content`. Serwis zwraca `bool`.
        - Odpowiedź (błąd): `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`.

## 8. Interakcje użytkownika
- **Wejście na stronę:** Widok inicjalizuje ładowanie danych (`IsLoading = true`), wywołuje `GetNotesAsync` z domyślnymi parametrami (strona 1, sortowanie wg daty modyfikacji malejąco).
- **Zmiana strony:** Użytkownik klika przycisk w `PaginationControls`. Komponent wywołuje `OnPageChanged` w `NotesList`. `NotesList` aktualizuje `CurrentPage` w stanie, ustawia `IsLoading = true` i wywołuje `GetNotesAsync` z nowym numerem strony.
- **Zmiana sortowania:** Użytkownik klika nagłówek w `SortableTableHeader`. Komponent wywołuje `OnSortChanged` w `NotesList`. `NotesList` aktualizuje `SortBy` i `SortDirection` w stanie, resetuje `CurrentPage` do 1, ustawia `IsLoading = true` i wywołuje `GetNotesAsync` z nowymi parametrami sortowania.
- **Inicjacja usunięcia:** Użytkownik klika ikonę/przycisk "Usuń" przy notatce. `NotesList` ustawia `NoteToDeleteId` na ID tej notatki i `IsDeleteModalVisible = true`, co powoduje wyświetlenie `DeleteConfirmationModal`.
- **Potwierdzenie usunięcia:** Użytkownik klika "Potwierdź" w modalu. `DeleteConfirmationModal` wywołuje `OnConfirm` w `NotesList`. `NotesList` wywołuje `DeleteNoteAsync` z zapamiętanym `NoteToDeleteId`. Po sukcesie: `IsDeleteModalVisible = false`, `NoteToDeleteId = null`, odświeżenie listy notatek (ponowne wywołanie `GetNotesAsync`).
- **Anulowanie usunięcia:** Użytkownik klika "Anuluj" w modalu lub zamyka go. `DeleteConfirmationModal` wywołuje `OnCancel` w `NotesList`. `NotesList` ustawia `IsDeleteModalVisible = false` i `NoteToDeleteId = null`.
- **Kliknięcie "Utwórz pierwszą notatkę":** (W `EmptyState`) Użycie `NavigationManager` do nawigacji pod adres `/notes/create`.

## 9. Warunki i walidacja
- **Wyświetlanie `LoadingSpinner`:** Renderowany gdy `NotesListViewState.IsLoading` jest `true`.
- **Wyświetlanie `EmptyState`:** Renderowany gdy `NotesListViewState.IsLoading` jest `false`, `NotesListViewState.ErrorMessage` jest `null` i (`NotesListViewState.NotesData` jest `null` lub `NotesListViewState.NotesData.TotalItems == 0`).
- **Wyświetlanie tabeli z notatkami:** Renderowana gdy `NotesListViewState.IsLoading` jest `false`, `NotesListViewState.ErrorMessage` jest `null` i `NotesListViewState.NotesData?.TotalItems > 0`.
- **Wyświetlanie `PaginationControls`:** Renderowany gdy warunki dla tabeli są spełnione ORAZ `NotesListViewState.NotesData.TotalPages > 1`.
- **Wyświetlanie `DeleteConfirmationModal`:** Renderowany (kontrolowany przez modal) gdy `NotesListViewState.IsDeleteModalVisible` jest `true`.
- **Wyświetlanie komunikatu o błędzie:** Renderowany gdy `NotesListViewState.ErrorMessage` nie jest `null`.
- **Deaktywacja przycisków paginacji:** Przycisk "Poprzednia" jest `disabled` gdy `NotesListViewState.CurrentPage <= 1`. Przycisk "Następna" jest `disabled` gdy `NotesListViewState.CurrentPage >= NotesListViewState.NotesData.TotalPages`.
- **Walidacja parametrów API:** Parametry `page`, `pageSize`, `sortBy`, `sortDirection` są walidowane logiką komponentu `NotesList` przed wywołaniem API, aby zapewnić zgodność z oczekiwaniami API (np. `page >= 1`, `sortBy` należy do dozwolonych wartości).

## 10. Obsługa błędów
- **Błąd pobierania listy (`GetNotesAsync`):** W bloku `catch` wywołania `INoteService.GetNotesAsync`, ustaw `NotesListViewState.IsLoading = false` i przypisz odpowiedni komunikat do `NotesListViewState.ErrorMessage`. Wyświetl ten komunikat użytkownikowi w dedykowanym miejscu UI. Można dodać przycisk "Spróbuj ponownie", który wywoła metodę ładującą dane.
- **Błąd usuwania (`DeleteNoteAsync`):** W bloku `catch` wywołania `INoteService.DeleteNoteAsync`:
    - Zamknij modal: `IsDeleteModalVisible = false`, `NoteToDeleteId = null`.
    - Ustaw `NotesListViewState.ErrorMessage` (lub użyj systemu powiadomień typu "toast") z komunikatem o błędzie (np. "Nie udało się usunąć notatki.", "Nie masz uprawnień.", "Notatka nie została znaleziona.").
    - W przypadku błędu 404 ("Not Found"), warto rozważyć odświeżenie listy, aby usunąć nieistniejącą notatkę z widoku. W innych przypadkach (np. błąd sieci, serwera) niekoniecznie trzeba odświeżać listę.
    - Zaloguj szczegóły błędu po stronie klienta lub wyślij do systemu monitorowania.
- **Nieautoryzowany dostęp (401):** Globalny mechanizm obsługi błędów aplikacji powinien przechwycić 401 i przekierować użytkownika do strony logowania.
- **Brak uprawnień (403):** Wyświetl odpowiedni komunikat błędu użytkownikowi (np. z `ErrorMessage` lub powiadomienia).

## 11. Kroki implementacji
1.  **Utworzenie pliku komponentu:** Stwórz plik `Pages/Notes/NotesList.razor`.
2.  **Definicja routingu i wstrzykiwanie zależności:** Dodaj dyrektywę `@page "/notes"` oraz `@inject INoteService NoteService` i `@inject NavigationManager NavigationManager`. Dodaj atrybut `@attribute [Authorize]` dla wymaganej autoryzacji.
3.  **Implementacja stanu:** Zdefiniuj klasę `NotesListViewState` (lub pola) w bloku `@code` do przechowywania stanu (loading, error, data, pagination, sort, modal).
4.  **Pobieranie danych:** Zaimplementuj metodę `LoadNotesAsync` wywoływaną w `OnInitializedAsync` oraz przy zmianie paginacji/sortowania. Metoda powinna:
    - Ustawić `IsLoading = true`.
    - Zbudować obiekt `GetNotesListQuery` na podstawie stanu.
    - Wywołać `NoteService.GetNotesAsync`.
    - W bloku `try`: zaktualizować `NotesData` i inne pola stanu.
    - W bloku `catch`: ustawić `ErrorMessage`.
    - W bloku `finally`: ustawić `IsLoading = false`.
5.  **Struktura HTML i renderowanie warunkowe:** Zaimplementuj strukturę HTML w sekcji `@* HTML *@` (`<Template>` w Blazor) z użyciem `@if`, `@else if`, `@else` do warunkowego renderowania `LoadingSpinner`, `EmptyState`, tabeli z notatkami i komunikatów o błędach na podstawie `NotesListViewState`.
6.  **Implementacja tabeli:** W przypadku istnienia danych (`NotesData.Items`), wyrenderuj tabelę (`<table>` z klasami Bootstrap).
    - **Nagłówki:** Użyj komponentu `SortableTableHeader` dla kolumn Tytuł, Data utworzenia, Data modyfikacji, przekazując odpowiednie propsy i obsługując zdarzenie `OnSortChanged`. Dodaj statyczny nagłówek dla Akcji.
    - **Wiersze:** Użyj pętli `@foreach (var note in NotesData.Items)` do wyrenderowania wierszy (`<tr>`) i komórek (`<td>`) dla każdej notatki, wyświetlając `note.Title`, `note.CreatedAt`, `note.ModifiedAt`. W ostatniej komórce dodaj przycisk/ikonę "Usuń" z `@onclick` wywołującym metodę otwierającą modal (np. `ShowDeleteModal(note.Id)`).
7.  **Implementacja komponentu `SortableTableHeader`:** Stwórz komponent `Shared/SortableTableHeader.razor` implementujący logikę zmiany sortowania i wywoływania `EventCallback`.
8.  **Implementacja komponentu `PaginationControls`:** Stwórz komponent `Shared/PaginationControls.razor` wyświetlający przyciski, obsługujący logikę deaktywacji i wywołujący `EventCallback`. Zintegruj go w `NotesList.razor` pod tabelą, renderując warunkowo.
9.  **Implementacja komponentu `DeleteConfirmationModal`:** Stwórz komponent `Shared/DeleteConfirmationModal.razor` (np. używając modala Bootstrap), który będzie kontrolowany przez `IsVisible` i wywoływał `OnConfirm`/`OnCancel`.
10. **Integracja modala usuwania:** W `NotesList.razor`:
    - Dodaj instancję `<DeleteConfirmationModal>` z powiązanymi parametrami (`IsVisible`, `OnConfirm`, `OnCancel`).
    - Zaimplementuj metodę `ShowDeleteModal(Guid noteId)`, która ustawia `NoteToDeleteId` i `IsDeleteModalVisible = true`.
    - Zaimplementuj metodę obsługującą `OnConfirm` (np. `ConfirmDelete`), która wywołuje `NoteService.DeleteNoteAsync`, obsługuje błędy i odświeża listę.
    - Zaimplementuj metodę obsługującą `OnCancel` (np. `CancelDelete`), która ustawia `IsDeleteModalVisible = false` i `NoteToDeleteId = null`.
11. **Implementacja `EmptyState`:** Użyj/stwórz komponent `Shared/EmptyState.razor` i renderuj go warunkowo. Dodaj w nim przycisk z `@onclick` używający `NavigationManager.NavigateTo("/notes/create")`.
12. **Styling:** Zastosuj klasy Bootstrap do tabeli, przycisków, modala, paginacji, aby zapewnić spójny wygląd zgodny z resztą aplikacji.
13. **Testowanie:** Przetestuj wszystkie ścieżki interakcji: ładowanie, pusty stan, wyświetlanie listy, sortowanie (wszystkie kolumny i kierunki), paginację, proces usuwania (otwarcie modala, anulowanie, potwierdzenie), obsługę błędów API. 