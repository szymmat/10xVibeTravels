# Plan implementacji widoku PlanDetails

## 1. Przegląd
Widok `PlanDetails` służy do wyświetlania szczegółowych informacji o pojedynczym planie podróży (wygenerowanym, zaakceptowanym lub odrzuconym). Umożliwia użytkownikowi przeglądanie pełnej treści planu, edycję treści dla planów zaakceptowanych oraz usunięcie planu. Widok jest dostępny tylko dla zalogowanych użytkowników i zapewnia, że użytkownicy mogą modyfikować lub usuwać tylko swoje plany.

## 2. Routing widoku
Widok powinien być dostępny pod następującą ścieżką:
`/plans/{id:guid}`
Gdzie `{id:guid}` jest identyfikatorem (GUID) planu do wyświetlenia.

## 3. Struktura komponentów
```
PlanDetails.razor (@page "/plans/{id:guid}")
├── @attribute [Authorize]
├── @inject HttpClient Http (lub dedykowany serwis np. IPlanDataService)
├── @inject NavigationManager NavigationManager
├── @inject IToastService ToastService
├── @inject IJSRuntime JSRuntime (potencjalnie dla modala)
│
├── @if (isLoading)
│   └── <LoadingSpinner />
├── @else if (errorMessage != null)
│   └── <div class="alert alert-danger">@errorMessage</div>
├── @else if (plan != null)
│   └── (Wyświetlanie Danych Planu)
│       ├── <h1>@plan.Title</h1>
│       ├── <span class="badge @GetStatusBadgeClass(plan.Status)">@plan.Status.ToString()</span>
│       ├── <p>Data rozpoczęcia: @plan.StartDate.ToString("yyyy-MM-dd")</p>
│       ├── <p>Data zakończenia: @plan.EndDate.ToString("yyyy-MM-dd")</p>
│       ├── <p>Budżet: @plan.Budget.ToString("C")</p>
│       ├── <p>Utworzono: @plan.CreatedAt.ToString("yyyy-MM-dd HH:mm")</p>
│       ├── <p>Zmodyfikowano: @plan.ModifiedAt.ToString("yyyy-MM-dd HH:mm")</p>
│       │
│       ├── @if (isEditing)
│       │   ├── <InputTextArea @bind-Value="editedContent" class="form-control" rows="10" />
│       │   ├── <button class="btn btn-primary" @onclick="SaveChanges" disabled="@isLoading">Zapisz Zmiany</button>
│       │   └── <button class="btn btn-secondary" @onclick="CancelEdit" disabled="@isLoading">Anuluj</button>
│       ├── @else
│       │   └── <div class="plan-content">@((MarkupString)plan.Content)</div> // Lub <pre> jeśli plain text
│       │   └── @if (plan.Status == PlanStatus.Accepted)
│       │       └── <button class="btn btn-secondary" @onclick="StartEditing" disabled="@isLoading">Edytuj Treść</button>
│       │
│       ├── <button class="btn btn-danger" @onclick="RequestDelete" disabled="@isLoading">Usuń Plan</button>
│       └── <DeleteConfirmationModal @ref="deleteConfirmationModal" OnConfirm="HandleDeleteConfirmed" Title="Potwierdź Usunięcie" Message="Czy na pewno chcesz usunąć ten plan?" />
│
└── (Logika @code)
```
*Zakłada istnienie komponentów `LoadingSpinner` i `DeleteConfirmationModal` oraz serwisu `IToastService`.*

## 4. Szczegóły komponentów
### `PlanDetails.razor`
-   **Opis komponentu:** Główny komponent strony, odpowiedzialny za pobieranie danych planu na podstawie ID z URL, wyświetlanie ich, obsługę trybu edycji treści dla zaakceptowanych planów oraz inicjowanie procesu usuwania.
-   **Główne elementy HTML i komponenty dzieci:**
    -   Standardowe elementy HTML (h1, p, span) do wyświetlania danych.
    -   Bootstrap `badge` do wyświetlania statusu.
    -   Warunkowo renderowany `InputTextArea` (dla edycji) lub `div`/`pre` (dla wyświetlania treści).
    -   Przyciski Bootstrap (`btn`) do akcji: "Edytuj Treść", "Zapisz Zmiany", "Anuluj", "Usuń Plan".
    -   Komponent `LoadingSpinner` (warunkowo).
    -   Komponent `DeleteConfirmationModal` (do potwierdzenia usunięcia).
-   **Obsługiwane interakcje:**
    -   Ładowanie danych przy inicjalizacji.
    -   Kliknięcie "Edytuj Treść": Przełącza do trybu edycji.
    -   Kliknięcie "Zapisz Zmiany": Wysyła żądanie `PATCH`, aktualizuje stan, przełącza do trybu widoku, pokazuje Toast.
    -   Kliknięcie "Anuluj": Odrzuca zmiany, przełącza do trybu widoku.
    -   Kliknięcie "Usuń Plan": Otwiera modal potwierdzenia.
    -   Potwierdzenie w modalu: Wysyła żądanie `DELETE`, nawiguje do listy planów, pokazuje Toast.
-   **Obsługiwana walidacja:**
    -   Sprawdzenie, czy `plan.Status == PlanStatus.Accepted` przed umożliwieniem edycji treści (przycisk "Edytuj Treść" jest widoczny tylko wtedy).
    -   Backend dodatkowo weryfikuje uprawnienia (własność planu) i logikę biznesową (np. czy można edytować/usuwać plan w danym statusie).
-   **Typy:** `PlanDetailDto`, `UpdatePlanDto`, `PlanStatus`, `Guid`.
-   **Propsy:** `[Parameter] public Guid Id { get; set; }`.

### `DeleteConfirmationModal.razor` (Zakładany)
-   **Opis komponentu:** Komponent modalny wielokrotnego użytku do potwierdzania akcji usuwania.
-   **Główne elementy:** Struktura modala Bootstrap, przyciski "Potwierdź" i "Anuluj".
-   **Obsługiwane interakcje:** Wyświetlanie/ukrywanie, emitowanie zdarzenia `OnConfirm` po kliknięciu "Potwierdź".
-   **Obsługiwana walidacja:** Brak.
-   **Typy:** Brak specyficznych (może przyjmować `string` dla tytułu i wiadomości).
-   **Propsy:** `Title` (string), `Message` (string). Posiada publiczną metodę `Show()` i zdarzenie `EventCallback OnConfirm`.

## 5. Typy
-   **`PlanDetailDto` (`_10xVibeTravels.Responses`):** Obiekt transferu danych używany do pobierania i wyświetlania szczegółów planu.
    ```csharp
    public class PlanDetailDto
    {
        public Guid Id { get; set; }
        public PlanStatus Status { get; set; } // Odpowiada enum PlanStatus
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal Budget { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
    ```
-   **`UpdatePlanDto` (`_10xVibeTravels.Dtos`):** Obiekt transferu danych używany do wysyłania aktualizacji planu (w tym widoku tylko dla pola `Content`).
    ```csharp
    public class UpdatePlanDto
    {
        public string? Content { get; set; }
        // Title i Status nie będą ustawiane przez ten widok
        // public string? Title { get; set; }
        // public PlanStatus? Status { get; set; }
    }
    ```
-   **`PlanStatus` (`_10xVibeTravels.Data`):** Enum definiujący możliwe statusy planu. Musi być dostępny w projekcie Blazor.
    ```csharp
    public enum PlanStatus { Generated, Accepted, Rejected }
    ```

## 6. Zarządzanie stanem
-   Stan widoku będzie zarządzany lokalnie w komponencie `PlanDetails.razor`.
-   **Kluczowe zmienne stanu:**
    -   `plan` (typu `PlanDetailDto?`): Przechowuje dane aktualnie wyświetlanego planu.
    -   `isLoading` (typu `bool`): Flaga wskazująca, czy trwa operacja asynchroniczna (ładowanie danych, zapis, usunięcie).
    -   `isEditing` (typu `bool`): Flaga wskazująca, czy aktywny jest tryb edycji treści.
    -   `editedContent` (typu `string?`): Przechowuje treść planu podczas edycji.
    -   `errorMessage` (typu `string?`): Przechowuje komunikaty o błędach krytycznych (np. błąd ładowania).
    -   Referencja do modala: `deleteConfirmationModal` (typu `DeleteConfirmationModal`).
-   Nie przewiduje się potrzeby tworzenia niestandardowych hooków (custom hooks) dla tego widoku. Stan jest stosunkowo prosty.
-   Serwisy takie jak `HttpClient` (lub dedykowany serwis danych), `NavigationManager`, `IToastService` będą wstrzykiwane (`@inject`).

## 7. Integracja API
Komponent będzie komunikował się z następującymi punktami końcowymi API:

-   **Pobieranie danych planu:**
    -   **Endpoint:** `GET /plans/{id}`
    -   **Akcja:** Wywoływana w `OnInitializedAsync`.
    -   **Typ odpowiedzi:** `PlanDetailDto`.
    -   **Obsługa błędów:** 404 (Not Found), 403 (Forbidden), 500 (Server Error).
-   **Aktualizacja treści planu:**
    -   **Endpoint:** `PATCH /plans/{id}`
    -   **Akcja:** Wywoływana po kliknięciu "Zapisz Zmiany".
    -   **Typ żądania:** `UpdatePlanDto` (zawierający tylko `Content`).
    -   **Typ odpowiedzi:** `PlanDetailDto` (zaktualizowany plan).
    -   **Obsługa błędów:** 400 (Bad Request - np. próba edycji planu w złym stanie), 404, 403, 500.
-   **Usuwanie planu:**
    -   **Endpoint:** `DELETE /plans/{id}`
    -   **Akcja:** Wywoływana po potwierdzeniu w modalu.
    -   **Typ odpowiedzi:** Brak (204 No Content).
    -   **Obsługa błędów:** 404, 403, 500.

Do komunikacji z API zostanie użyty wstrzyknięty `HttpClient` lub dedykowany, typowany klient/serwis (np. `IPlanDataService`).

## 8. Interakcje użytkownika
-   **Wejście na stronę:** Użytkownik nawiguje do `/plans/{id}`. Widok ładuje dane, pokazując spinner. Po załadowaniu wyświetlane są szczegóły planu lub komunikat błędu.
-   **Kliknięcie "Edytuj Treść" (dla PlanStatus.Accepted):** Statyczny widok treści jest zastępowany przez `InputTextArea` z aktualną treścią. Pojawiają się przyciski "Zapisz Zmiany" i "Anuluj", a przycisk "Edytuj Treść" znika.
-   **Edycja treści:** Użytkownik modyfikuje tekst w `InputTextArea`. Zmiany są przechowywane w zmiennej `editedContent`.
-   **Kliknięcie "Zapisz Zmiany":** Pokazuje się spinner. Żądanie `PATCH` jest wysyłane do API.
    -   **Sukces:** Spinner znika, widok wraca do trybu wyświetlania ze zaktualizowaną treścią, pojawia się toast sukcesu.
    -   **Błąd:** Spinner znika, widok pozostaje w trybie edycji, pojawia się toast błędu.
-   **Kliknięcie "Anuluj":** Widok wraca do trybu wyświetlania, zmiany w `editedContent` są odrzucane.
-   **Kliknięcie "Usuń Plan":** Otwiera się modal `DeleteConfirmationModal` z prośbą o potwierdzenie.
-   **Kliknięcie "Potwierdź" w modalu:** Modal znika, pokazuje się spinner. Żądanie `DELETE` jest wysyłane do API.
    -   **Sukces:** Spinner znika, użytkownik jest przekierowywany na listę planów (`/plans`), pojawia się toast sukcesu.
    -   **Błąd:** Spinner znika, modal jest już zamknięty, pojawia się toast błędu.
-   **Kliknięcie "Anuluj" w modalu:** Modal znika, nic się nie dzieje.

## 9. Warunki i walidacja
-   **Dostęp:** Widok chroniony atrybutem `[Authorize]`. Dostęp tylko dla zalogowanych użytkowników.
-   **Identyfikator planu:** Walidacja formatu GUID jest zapewniona przez constraint routingu (`{id:guid}`).
-   **Istnienie planu i uprawnienia:** Weryfikowane po stronie API przy każdym żądaniu (`GET`, `PATCH`, `DELETE`). Frontend reaguje na błędy 404 (nie znaleziono) i 403 (brak uprawnień), wyświetlając odpowiedni `errorMessage` lub `Toast`.
-   **Możliwość edycji:** Przycisk "Edytuj Treść" jest renderowany tylko gdy `plan?.Status == PlanStatus.Accepted`. Logika zapisu (`SaveChanges`) również powinna być wywoływana tylko w tym stanie, chociaż główna walidacja logiki biznesowej (czy można edytować dany plan) leży po stronie API.
-   **Payload aktualizacji:** Upewnić się, że obiekt `UpdatePlanDto` wysyłany w żądaniu `PATCH` zawiera *tylko* zaktualizowane pole `Content`, gdy celem jest edycja treści zaakceptowanego planu.

## 10. Obsługa błędów
-   **Błędy ładowania danych (`GET /plans/{id}`):**
    -   `404 Not Found`: Wyświetlić komunikat "Plan nie został znaleziony." w głównym obszarze widoku (`errorMessage`).
    -   `403 Forbidden`: Wyświetlić komunikat "Brak uprawnień do wyświetlenia tego planu." (`errorMessage`).
    -   `500 Server Error / Network Error`: Wyświetlić generyczny komunikat "Wystąpił błąd serwera podczas ładowania planu." (`errorMessage`).
-   **Błędy zapisu zmian (`PATCH /plans/{id}`):**
    -   `400 Bad Request`: Wyświetlić `Toast` błędu (np. "Nie można zapisać zmian. Sprawdź dane lub status planu."). Pozostać w trybie edycji.
    -   `404 / 403`: Wyświetlić `Toast` błędu (np. "Błąd zapisu: Plan nie istnieje lub brak uprawnień."). Pozostać w trybie edycji.
    -   `500 Server Error / Network Error`: Wyświetlić `Toast` błędu ("Wystąpił błąd serwera podczas zapisywania."). Pozostać w trybie edycji.
-   **Błędy usuwania (`DELETE /plans/{id}`):**
    -   `404 / 403`: Wyświetlić `Toast` błędu ("Błąd usuwania: Plan nie istnieje lub brak uprawnień.").
    -   `500 Server Error / Network Error`: Wyświetlić `Toast` błędu ("Wystąpił błąd serwera podczas usuwania.").
-   **Stan ładowania:** Używać flagi `isLoading` do wyłączania przycisków podczas operacji API, aby zapobiec podwójnym kliknięciom. Wyświetlać `LoadingSpinner` dla wizualnej informacji zwrotnej.
-   **Powiadomienia:** Używać wstrzykniętego `IToastService` do informowania użytkownika o sukcesie lub błędach operacji zapisu i usuwania.

## 11. Kroki implementacji
1.  **Utworzenie pliku komponentu:** Stworzyć plik `PlanDetails.razor` w odpowiednim folderze (np. `Pages/Plans`).
2.  **Routing i autoryzacja:** Dodać dyrektywę `@page "/plans/{id:guid}"` i atrybut `@attribute [Authorize]`.
3.  **Parametr ID:** Zdefiniować parametr `[Parameter] public Guid Id { get; set; }`.
4.  **Wstrzykiwanie zależności:** Dodać dyrektywy `@inject` dla `HttpClient` (lub serwisu danych), `NavigationManager`, `IToastService` i potencjalnie `IJSRuntime`.
5.  **Definicja stanu:** W sekcji `@code` zdefiniować zmienne stanu: `plan`, `isLoading`, `isEditing`, `editedContent`, `errorMessage` oraz referencję do modala.
6.  **Logika ładowania danych:** Zaimplementować metodę `OnInitializedAsync` do pobierania danych planu z API (`GET /plans/{id}`), obsługi stanów ładowania (`isLoading`) i błędów (`errorMessage`).
7.  **Struktura HTML:** Zbudować strukturę HTML do wyświetlania danych planu (`plan.Title`, `plan.StartDate`, etc.) oraz statusu (`badge`). Użyć warunkowego renderowania (`@if`) do obsługi stanów `isLoading`, `errorMessage` i `plan == null`.
8.  **Implementacja trybu edycji:**
    -   Dodać przycisk "Edytuj Treść" widoczny tylko dla `plan.Status == PlanStatus.Accepted`.
    -   Dodać blok `@if (isEditing)` zawierający `InputTextArea` powiązany z `editedContent` oraz przyciski "Zapisz Zmiany" i "Anuluj".
    -   Implementować metody `StartEditing()`, `CancelEdit()`.
9.  **Implementacja zapisu zmian:**
    -   Implementować metodę `SaveChanges()`, która tworzy `UpdatePlanDto`, wywołuje `PATCH /plans/{id}`, obsługuje odpowiedź sukcesu (aktualizacja `plan`, wyjście z trybu edycji, `Toast`) i błędu (`Toast`). Ustawiać `isLoading`.
10. **Implementacja usuwania:**
    -   Dodać przycisk "Usuń Plan".
    -   Dodać komponent `DeleteConfirmationModal` do markupu i uzyskać do niego referencję.
    -   Implementować metodę `RequestDelete()`, która pokazuje modal.
    -   Implementować metodę `HandleDeleteConfirmed()`, która jest wywoływana przez zdarzenie `OnConfirm` modala. Metoda ta wywołuje `DELETE /plans/{id}`, obsługuje sukces (nawigacja, `Toast`) i błąd (`Toast`). Ustawiać `isLoading`.
11. **Styling:** Dodać klasy Bootstrap i ewentualnie niestandardowe style CSS dla wyglądu (np. kolory badge'y statusu, formatowanie treści).
12. **Testowanie:** Przetestować wszystkie ścieżki: ładowanie, różne statusy planów, edycję (tylko dla Accepted), zapis, anulowanie edycji, usuwanie, obsługę błędów API (404, 403, 500). 