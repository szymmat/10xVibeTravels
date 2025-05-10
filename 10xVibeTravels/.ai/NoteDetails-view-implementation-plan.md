# Plan implementacji widoku Szczegółów Notatki (NoteDetails)

## 1. Przegląd
Widok `NoteDetails` służy do wyświetlania pełnych informacji o pojedynczej notatce użytkownika. Umożliwia on przeglądanie tytułu, treści oraz dat utworzenia i modyfikacji notatki. Dodatkowo, widok ten stanowi punkt wejścia do edycji notatki, jej usunięcia oraz zainicjowania procesu generowania planu podróży AI na podstawie tej notatki. Widok wymaga autoryzacji i zapewnia dostęp tylko do notatek należących do zalogowanego użytkownika.

## 2. Routing widoku
Widok będzie dostępny pod ścieżką:
`/notes/{id:guid}`
Gdzie `{id}` jest identyfikatorem GUID notatki.

## 3. Struktura komponentów
```
NoteDetails.razor (Komponent strony, Route: /notes/{id})
├── LoadingSpinner.razor (Warunkowo: podczas ładowania danych)
├── div (Wyświetlanie danych notatki - Warunkowo: po załadowaniu, jeśli notatka istnieje)
│   ├── h3 (Tytuł notatki)
│   ├── p (Treść notatki - pre-formatowana dla zachowania białych znaków)
│   ├── small (Data utworzenia)
│   ├── small (Data modyfikacji)
├── div (Przyciski akcji - Warunkowo: po załadowaniu, jeśli notatka istnieje)
│   ├── button "Edytuj Notatkę" (Otwiera EditNoteModal)
│   ├── button "Usuń Notatkę" (Otwiera DeleteConfirmationModal)
│   ├── button "Generuj Plan" (Otwiera GeneratePlanModal)
├── div (Komunikat błędu - Warunkowo: jeśli wystąpił błąd ładowania/dostępu)
├── EditNoteModal.razor (Warunkowo: gdy isEditModalOpen == true)
├── DeleteConfirmationModal.razor (Warunkowo: gdy isDeleteModalOpen == true)
└── GeneratePlanModal.razor (Warunkowo: gdy isGenerateModalOpen == true)
```

## 4. Szczegóły komponentów

### `NoteDetails.razor`
*   **Opis:** Główny komponent strony. Odpowiedzialny za pobranie danych notatki na podstawie parametru `id` z URL, wyświetlenie ich, zarządzanie stanem ładowania i błędów oraz koordynację otwierania modali akcji (edycja, usunięcie, generowanie planu).
*   **Główne elementy:** Kontener dla wyświetlania danych notatki (tytuł, treść, daty), sekcja z przyciskami akcji, miejsce na wyświetlenie komunikatu błędu, wywołania komponentów modali. Wykorzystuje `@inject INoteService`, `@inject NavigationManager`, `@inject AuthenticationStateProvider`.
*   **Obsługiwane interakcje:**
    *   Ładowanie danych notatki przy inicjalizacji (`OnInitializedAsync`).
    *   Otwieranie modala edycji po kliknięciu "Edytuj Notatkę".
    *   Otwieranie modala potwierdzenia usunięcia po kliknięciu "Usuń Notatkę".
    *   Otwieranie modala generowania planu po kliknięciu "Generuj Plan".
    *   Obsługa zdarzeń zwrotnych z modali (np. po zapisaniu edycji, potwierdzeniu usunięcia).
*   **Obsługiwana walidacja:** Sprawdzenie poprawności GUID w routingu (przez dyrektywę `:guid`). Weryfikacja uprawnień i istnienia notatki odbywa się na podstawie odpowiedzi z API.
*   **Typy:** `NoteDto? CurrentNote`, `bool IsLoading`, `string? ErrorMessage`, `Guid NoteId` (parametr).
*   **Propsy (Parametry):** `[Parameter] public Guid NoteId { get; set; }`.

### `EditNoteModal.razor`
*   **Opis:** Komponent modalny zawierający formularz `EditForm` do edycji tytułu i treści notatki. Odpowiada za walidację wprowadzonych danych i komunikację z komponentem nadrzędnym w celu zapisu lub anulowania zmian.
*   **Główne elementy:** Bootstrap modal (`.modal`, `.modal-dialog`, `.modal-content`, etc.), Blazor `EditForm`, `InputText` dla tytułu, `InputTextArea` dla treści, przyciski "Zapisz" i "Anuluj", `ValidationSummary` lub `ValidationMessage` do wyświetlania błędów walidacji.
*   **Obsługiwane interakcje:**
    *   Wypełnienie pól formularza.
    *   Walidacja pól przy zmianie i próbie zapisu.
    *   Wywołanie `OnSave` przy kliknięciu "Zapisz" (po pomyślnej walidacji).
    *   Wywołanie `OnCancel` przy kliknięciu "Anuluj" lub zamknięciu modala.
*   **Obsługiwana walidacja:**
    *   `Tytuł`: Wymagany (`[Required]`), Maksymalna długość 100 znaków (`[MaxLength(100)]`).
    *   `Treść`: Wymagana (`[Required]`), Maksymalna długość 2000 znaków (`[MaxLength(2000)]`).
*   **Typy:** `NoteEditViewModel NoteModel`.
*   **Propsy (Parametry):**
    *   `[Parameter] public NoteDto InitialNoteData { get; set; }` (Do wstępnego wypełnienia formularza).
    *   `[Parameter] public EventCallback<NoteEditViewModel> OnSave { get; set; }` (Wywoływane przy zapisie).
    *   `[Parameter] public EventCallback OnCancel { get; set; }` (Wywoływane przy anulowaniu).
    *   `[Parameter] public bool IsVisible { get; set; }` (Do kontrolowania widoczności modala - alternatywnie, zarządzanie widocznością może być wewnętrzne).

### `DeleteConfirmationModal.razor`
*   **Opis:** Prosty komponent modalny wyświetlający pytanie potwierdzające usunięcie notatki.
*   **Główne elementy:** Bootstrap modal, tekst pytania (np. "Czy na pewno chcesz usunąć tę notatkę?"), przyciski "Potwierdź" i "Anuluj".
*   **Obsługiwane interakcje:**
    *   Kliknięcie "Potwierdź" -> wywołuje `OnConfirmation(true)`.
    *   Kliknięcie "Anuluj" -> wywołuje `OnConfirmation(false)`.
*   **Obsługiwana walidacja:** Brak.
*   **Typy:** Brak specyficznych.
*   **Propsy (Parametry):**
    *   `[Parameter] public EventCallback<bool> OnConfirmation { get; set; }`.
    *   `[Parameter] public bool IsVisible { get; set; }`.

### `GeneratePlanModal.razor`
*   **Opis:** Komponent modalny z formularzem `EditForm` do zbierania danych wejściowych (daty, budżet) potrzebnych do zainicjowania generowania planu AI.
*   **Główne elementy:** Bootstrap modal, Blazor `EditForm`, `InputDate` dla daty początkowej i końcowej, `InputNumber` dla budżetu, przyciski "Generuj" i "Anuluj", komponenty walidacji.
*   **Obsługiwane interakcje:**
    *   Wybór dat i wprowadzenie budżetu.
    *   Walidacja danych.
    *   Wywołanie `OnGenerate` z danymi wejściowymi po kliknięciu "Generuj".
    *   Wywołanie `OnCancel` po kliknięciu "Anuluj".
*   **Obsługiwana walidacja:**
    *   `StartDate`: Wymagane.
    *   `EndDate`: Wymagane, musi być późniejsze niż `StartDate` (implementacja za pomocą `IValidatableObject` lub niestandardowego atrybutu walidacji).
    *   `Budget`: Musi być wartością nieujemną (>= 0), jeśli została podana (`[Range(0, double.MaxValue)]`).
*   **Typy:** `GeneratePlanInputModel PlanInputData`.
*   **Propsy (Parametry):**
    *   `[Parameter] public EventCallback<GeneratePlanInputModel> OnGenerate { get; set; }`.
    *   `[Parameter] public EventCallback OnCancel { get; set; }`.
    *   `[Parameter] public bool IsVisible { get; set; }`.

### `LoadingSpinner.razor`
*   **Opis:** Prosty, współdzielony komponent wyświetlający animację ładowania (np. spinner Bootstrap).
*   **Główne elementy:** `div` z klasami Bootstrap `spinner-border` lub podobnymi.
*   **Obsługiwane interakcje:** Brak.
*   **Obsługiwana walidacja:** Brak.
*   **Typy:** Brak.
*   **Propsy (Parametry):** Brak.

## 5. Typy

*   **`NoteDto` (DTO z backendu):**
    *   `Id`: `Guid` - Identyfikator notatki.
    *   `Title`: `string` - Tytuł notatki.
    *   `Content`: `string` - Treść notatki.
    *   `CreatedAt`: `DateTime` - Data utworzenia (UTC).
    *   `ModifiedAt`: `DateTime` - Data ostatniej modyfikacji (UTC).

*   **`UpdateNoteRequest` (DTO do backendu dla PUT /notes/{id}):**
    *   `Title`: `string` - Zaktualizowany tytuł.
    *   `Content`: `string` - Zaktualizowana treść.

*   **`NoteEditViewModel` (ViewModel dla `EditNoteModal`):**
    *   `[Required(ErrorMessage = "Tytuł jest wymagany.")]`
    *   `[MaxLength(100, ErrorMessage = "Tytuł nie może przekraczać 100 znaków.")]`
    *   `public string Title { get; set; }`
    *   `[Required(ErrorMessage = "Treść jest wymagana.")]`
    *   `[MaxLength(2000, ErrorMessage = "Treść nie może przekraczać 2000 znaków.")]`
    *   `public string Content { get; set; }`

*   **`GeneratePlanInputModel` (ViewModel dla `GeneratePlanModal`):**
    *   `[Required(ErrorMessage = "Data początkowa jest wymagana.")]`
    *   `public DateTime? StartDate { get; set; }`
    *   `[Required(ErrorMessage = "Data końcowa jest wymagana.")]`
    *   `// Dodatkowa walidacja w komponencie lub przez IValidatableObject do sprawdzenia, czy EndDate > StartDate`
    *   `public DateTime? EndDate { get; set; }`
    *   `[Range(0, double.MaxValue, ErrorMessage = "Budżet musi być wartością nieujemną.")]`
    *   `public decimal? Budget { get; set; }`
    *   *(Implementacja `IValidatableObject` do walidacji EndDate względem StartDate)*

## 6. Zarządzanie stanem
Stan widoku `NoteDetails` będzie zarządzany głównie wewnątrz samego komponentu (`@code` block).
*   **Kluczowe zmienne stanu:**
    *   `currentNote (NoteDto?)`: Przechowuje dane załadowanej notatki.
    *   `isLoading (bool)`: Kontroluje wyświetlanie wskaźnika ładowania.
    *   `errorMessage (string?)`: Przechowuje komunikaty błędów do wyświetlenia.
    *   `isEditModalOpen`, `isDeleteModalOpen`, `isGenerateModalOpen (bool)`: Kontrolują widoczność odpowiednich modali.
*   **Aktualizacja stanu:** Stan będzie aktualizowany w odpowiedzi na cykl życia komponentu (`OnInitializedAsync`), interakcje użytkownika (kliknięcia przycisków) oraz wyniki wywołań API (sukces, błąd).
*   **Przepływ danych:** Dane notatki (`currentNote`) są przekazywane do `EditNoteModal` jako parametr `InitialNoteData`. Dane wejściowe z modali (`NoteEditViewModel`, `GeneratePlanInputModel`, `bool` z potwierdzenia usunięcia) są przekazywane z powrotem do `NoteDetails` za pomocą `EventCallback`.
*   **Niestandardowe hooki:** Nie są stosowane w Blazor. Odpowiednikiem jest wstrzykiwanie serwisów (`INoteService`, `NavigationManager`, `AuthenticationStateProvider`) i logika zarządzania stanem w bloku `@code`.

## 7. Integracja API
Integracja z backendem będzie realizowana poprzez wstrzyknięty serwis `INoteService`. Komponent `NoteDetails.razor` będzie potrzebował dostępu do `UserId` zalogowanego użytkownika, co zostanie uzyskane za pomocą `AuthenticationStateProvider`.

*   **Pobieranie szczegółów notatki:**
    *   **Akcja:** `OnInitializedAsync`.
    *   **Metoda serwisu:** `INoteService.GetNoteByIdAsync(userId, NoteId)`
    *   **Endpoint:** `GET /notes/{id}`
    *   **Typ odpowiedzi:** `NoteDto`
    *   **Obsługa sukcesu:** Przypisanie wyniku do `currentNote`, ustawienie `isLoading = false`.
    *   **Obsługa błędów:** Ustawienie `errorMessage` na podstawie statusu (404, 403) lub generyczny błąd (500), `isLoading = false`.

*   **Aktualizacja notatki:**
    *   **Akcja:** Handler dla `OnSave` z `EditNoteModal`.
    *   **Metoda serwisu:** `INoteService.UpdateNoteAsync(userId, noteId, updateRequest)`
    *   **Endpoint:** `PUT /notes/{id}`
    *   **Typ żądania:** `UpdateNoteRequest` (mapowany z `NoteEditViewModel`).
    *   **Typ odpowiedzi:** `NoteDto` (zaktualizowana notatka).
    *   **Obsługa sukcesu:** Aktualizacja `currentNote` nowymi danymi, zamknięcie modala.
    *   **Obsługa błędów:** Wyświetlenie błędu w modalu (400) lub na stronie (inne).

*   **Usuwanie notatki:**
    *   **Akcja:** Handler dla `OnConfirmation(true)` z `DeleteConfirmationModal`.
    *   **Metoda serwisu:** `INoteService.DeleteNoteAsync(userId, NoteId)`
    *   **Endpoint:** `DELETE /notes/{id}`
    *   **Typ odpowiedzi:** `bool` (sukces/porażka).
    *   **Obsługa sukcesu:** Zamknięcie modala, nawigacja do listy notatek (`/notes`).
    *   **Obsługa błędów:** Wyświetlenie błędu, zamknięcie modala.

*   **(Inicjacja) Generowanie planu:**
    *   **Akcja:** Handler dla `OnGenerate` z `GeneratePlanModal`.
    *   **Metoda serwisu:** (Zależne od implementacji) np. `IPlanGenerationService.InitiateGenerationAsync(userId, NoteId, generateInput)`
    *   **Endpoint:** (Zależne od implementacji) np. `POST /plan-generation`
    *   **Typ żądania:** (Zależne od implementacji, zawiera `NoteId` i dane z `GeneratePlanInputModel`).
    *   **Typ odpowiedzi:** (Zależne od implementacji, np. ID zadania generowania).
    *   **Obsługa sukcesu:** Zamknięcie modala, nawigacja do strony wyników/ładowania generowania.
    *   **Obsługa błędów:** Wyświetlenie błędu w modalu.

## 8. Interakcje użytkownika
*   **Ładowanie strony:** Użytkownik widzi spinner, a następnie dane notatki lub komunikat błędu.
*   **Kliknięcie "Edytuj Notatkę":** Otwiera się modal z formularzem wypełnionym danymi notatki.
*   **Edycja i zapis:** Użytkownik modyfikuje dane, klika "Zapisz". Jeśli walidacja przejdzie, dane są wysyłane do API. Po sukcesie modal się zamyka, a widok główny pokazuje zaktualizowane dane. W przypadku błędu walidacji lub API, błąd jest pokazywany w modalu.
*   **Edycja i anulowanie:** Użytkownik klika "Anuluj". Modal się zamyka, zmiany są odrzucane.
*   **Kliknięcie "Usuń Notatkę":** Otwiera się modal potwierdzenia.
*   **Potwierdzenie usunięcia:** Użytkownik klika "Potwierdź". API jest wywoływane. Po sukcesie modal się zamyka, użytkownik jest przekierowywany do listy notatek. W przypadku błędu API, błąd jest pokazywany, modal się zamyka.
*   **Anulowanie usunięcia:** Użytkownik klika "Anuluj". Modal się zamyka, nic się nie dzieje.
*   **Kliknięcie "Generuj Plan":** Otwiera się modal z formularzem dat i budżetu.
*   **Wypełnienie i generowanie:** Użytkownik wprowadza dane, klika "Generuj". Jeśli walidacja przejdzie, proces generowania jest inicjowany. Modal się zamyka, użytkownik jest przekierowywany lub widzi informację o rozpoczęciu procesu. W przypadku błędu walidacji lub API, błąd jest pokazywany w modalu.
*   **Anulowanie generowania:** Użytkownik klika "Anuluj". Modal się zamyka, nic się nie dzieje.

## 9. Warunki i walidacja
*   **Dostęp do widoku:** Wymaga autoryzacji (`[Authorize]` na stronie lub w kontrolerze API). Sprawdzane przez middleware ASP.NET Identity.
*   **Poprawność ID notatki:** Parametr `{id}` musi być poprawnym GUID. Walidowane przez routing Blazor (`:guid`). Niepoprawny format skutkuje stroną 404 Blazora.
*   **Istnienie i własność notatki:** Weryfikowane przez backend przy wywołaniach `GetNoteByIdAsync`, `UpdateNoteAsync`, `DeleteNoteAsync`. API zwraca 404 (Not Found) lub 403 (Forbidden). Frontend musi odpowiednio obsłużyć te statusy, wyświetlając `errorMessage`.
*   **Walidacja formularza edycji (`EditNoteModal`):**
    *   Tytuł: Wymagany, max 100 znaków.
    *   Treść: Wymagana, max 2000 znaków.
    *   Realizowane za pomocą DataAnnotations na `NoteEditViewModel` i komponentu `EditForm`. Komunikaty błędów wyświetlane obok pól lub w `ValidationSummary`. Przycisk "Zapisz" powinien być nieaktywny, jeśli formularz jest niepoprawny.
*   **Walidacja formularza generowania planu (`GeneratePlanModal`):**
    *   Data początkowa: Wymagana.
    *   Data końcowa: Wymagana, musi być po dacie początkowej.
    *   Budżet: Opcjonalny, ale jeśli podany, musi być >= 0.
    *   Realizowane za pomocą DataAnnotations i potencjalnie `IValidatableObject` na `GeneratePlanInputModel`. Komunikaty błędów wyświetlane. Przycisk "Generuj" nieaktywny, jeśli formularz jest niepoprawny.

## 10. Obsługa błędów
*   **Błędy ładowania danych (GET /notes/{id}):**
    *   **404 Not Found:** Wyświetlić `errorMessage`: "Nie znaleziono notatki."
    *   **403 Forbidden:** Wyświetlić `errorMessage`: "Brak uprawnień do tej notatki." (lub przekierować).
    *   **401 Unauthorized:** Powinno być obsłużone globalnie przez przekierowanie do logowania.
    *   **500 Internal Server Error / Inne:** Wyświetlić generyczny `errorMessage`: "Wystąpił błąd serwera/połączenia. Spróbuj ponownie później."
*   **Błędy aktualizacji danych (PUT /notes/{id}):**
    *   **400 Bad Request (Validation):** Wyświetlić szczegóły błędu walidacji w `EditNoteModal`.
    *   **404 Not Found / 403 Forbidden:** Wyświetlić odpowiedni błąd w modalu lub na stronie głównej.
    *   **Inne:** Wyświetlić generyczny błąd.
*   **Błędy usuwania danych (DELETE /notes/{id}):**
    *   **404 Not Found / 403 Forbidden:** Wyświetlić odpowiedni `errorMessage`.
    *   **Inne:** Wyświetlić generyczny `errorMessage`.
*   **Błędy inicjacji generowania planu:**
    *   **400 Bad Request (Validation):** Wyświetlić szczegóły błędu w `GeneratePlanModal`.
    *   **Inne:** Wyświetlić generyczny błąd w modalu.
*   **Ogólne:** Wskaźnik ładowania (`isLoading`) powinien być zarządzany poprawnie, ukrywając się po zakończeniu operacji (sukces lub błąd). Komunikaty błędów powinny być czytelne dla użytkownika.

## 11. Kroki implementacji
1.  **Utworzenie pliku komponentu:** Stwórz plik `NoteDetails.razor` w odpowiednim folderze (np. `Components/Pages/Notes`).
2.  **Dodanie routingu:** Dodaj dyrektywę `@page "/notes/{id:guid}"` na początku pliku.
3.  **Wstrzyknięcie zależności:** Dodaj dyrektywy `@inject` dla `INoteService`, `NavigationManager`, `AuthenticationStateProvider`.
4.  **Dodanie parametru:** Zdefiniuj parametr `[Parameter] public Guid NoteId { get; set; }`.
5.  **Implementacja logiki ładowania:** W `OnInitializedAsync`, pobierz `userId`, wywołaj `NoteSvc.GetNoteByIdAsync`, obsłuż stany `isLoading`, `currentNote` i `errorMessage` w bloku `try-catch`.
6.  **Struktura HTML i wyświetlanie danych:** Dodaj podstawowy markup HTML (używając Bootstrap). Warunkowo wyświetlaj `LoadingSpinner`, dane notatki (`currentNote.Title`, `currentNote.Content`, `currentNote.CreatedAt`, `currentNote.ModifiedAt`) lub `errorMessage`. Pamiętaj o `@preservewhitespace` lub `<pre>` dla treści notatki.
7.  **Przyciski akcji:** Dodaj przyciski "Edytuj Notatkę", "Usuń Notatkę", "Generuj Plan". Na razie ich `@onclick` mogą tylko ustawiać flagi widoczności modali (np. `isEditModalOpen = true`).
8.  **Stworzenie komponentów modali:** Stwórz pliki `EditNoteModal.razor`, `DeleteConfirmationModal.razor`, `GeneratePlanModal.razor` (np. w `Components/Shared/Modals`).
9.  **Implementacja `DeleteConfirmationModal`:** Dodaj prosty markup, parametry `IsVisible` i `OnConfirmation`. Dodaj logikę do przycisków wywołującą `OnConfirmation`.
10. **Integracja `DeleteConfirmationModal`:** W `NoteDetails.razor`, dodaj `<DeleteConfirmationModal ... />`, powiąż `IsVisible` ze stanem `isDeleteModalOpen`, zaimplementuj handler dla `OnConfirmation` (który wywoła `NoteSvc.DeleteNoteAsync` i obsłuży wynik/błędy, nawigację).
11. **Implementacja `EditNoteModal`:**
    *   Zdefiniuj `NoteEditViewModel` z DataAnnotations.
    *   Dodaj parametry `InitialNoteData`, `OnSave`, `OnCancel`, `IsVisible`.
    *   Stwórz `EditForm` powiązany z instancją `NoteEditViewModel`.
    *   Dodaj `InputText`, `InputTextArea`, przyciski "Zapisz", "Anuluj".
    *   Implementuj logikę `OnValidSubmit` (wywołuje `OnSave`) i logikę dla "Anuluj" (wywołuje `OnCancel`).
    *   Wypełnij model danymi z `InitialNoteData` przy inicjalizacji modala (np. w `OnParametersSet`).
12. **Integracja `EditNoteModal`:** W `NoteDetails.razor`, dodaj `<EditNoteModal ... />`, powiąż `IsVisible`, przekaż `currentNote` do `InitialNoteData`, zaimplementuj handler dla `OnSave` (mapuje ViewModel na `UpdateRequest`, wywołuje `NoteSvc.UpdateNoteAsync`, aktualizuje `currentNote`, zamyka modal, obsługuje błędy) i `OnCancel` (zamyka modal).
13. **Implementacja `GeneratePlanModal`:**
    *   Zdefiniuj `GeneratePlanInputModel` z DataAnnotations i walidacją `IValidatableObject` (lub niestandardowym atrybutem).
    *   Dodaj parametry `OnGenerate`, `OnCancel`, `IsVisible`.
    *   Stwórz `EditForm` powiązany z instancją `GeneratePlanInputModel`.
    *   Dodaj `InputDate`, `InputNumber`, przyciski "Generuj", "Anuluj".
    *   Implementuj logikę `OnValidSubmit` (wywołuje `OnGenerate`) i dla "Anuluj" (wywołuje `OnCancel`).
14. **Integracja `GeneratePlanModal`:** W `NoteDetails.razor`, dodaj `<GeneratePlanModal ... />`, powiąż `IsVisible`, zaimplementuj handler dla `OnGenerate` (wywołuje odpowiednią usługę generowania, obsługuje wynik/błędy, nawiguje/informuje użytkownika) i `OnCancel`.
15. **Styling i UX:** Dopracuj wygląd używając Bootstrap, zapewnij czytelne komunikaty błędów i płynne przejścia stanów (ładowanie, błąd, widok danych). Upewnij się, że przyciski są odpowiednio (nie)aktywne.
16. **Testowanie:** Przetestuj wszystkie ścieżki: ładowanie poprawne, notatka nie znaleziona, brak dostępu, edycja (sukces, walidacja, błąd API), usunięcie (sukces, anulowanie, błąd API), generowanie (sukces, walidacja, anulowanie, błąd API). 