# Plan implementacji widoku PlanProposalsView

## 1. Przegląd
Widok `PlanProposalsView` służy do prezentacji użytkownikowi trzech propozycji planów podróży wygenerowanych przez AI na podstawie jego notatki i preferencji. Umożliwia porównanie planów, edycję ich tytułów (przed akceptacją), akceptację jednego lub więcej planów (co zapisuje je jako osobne byty) lub ich odrzucenie. Widok obsługuje również możliwość ponownego wygenerowania propozycji, jeśli kontekst jest dostępny, oraz przekierowuje użytkownika do listy zapisanych planów po zakończeniu interakcji.

## 2. Routing widoku
Widok powinien być dostępny pod ścieżką `/plan-proposals`. Dostęp do widoku wymaga autoryzacji użytkownika. Pobieranie odpowiednich propozycji (np. ostatnio wygenerowanych dla użytkownika) musi zostać obsłużone w logice ładowania danych komponentu.

## 3. Struktura komponentów
```
PlanProposalsView.razor (@page "/plan-proposals")
├── @attribute [Authorize]
├── LoadingSpinner (Warunkowo, podczas ładowania danych)
├── ErrorMessageDisplay (Warunkowo, przy błędzie ładowania)
├── div.row.g-3 (Siatka Bootstrap)
│   ├── @foreach proposalVM in ProposalsList
│   │   └── div.col-lg-4.col-md-6.mb-4 (Kolumny responsywne)
│   │       └── ProposalCard Proposal="proposalVM" OnProposalUpdated="HandleProposalUpdate"
├── Button ("Zakończ i zobacz zapisane plany", Warunkowo, po interakcji)
└── (Integracja z ToastService przez @inject)
```

## 4. Szczegóły komponentów
### PlanProposalsView.razor
*   **Opis komponentu:** Główny komponent strony, odpowiedzialny za pobranie i wyświetlenie listy propozycji planów (np. ostatnio wygenerowanych dla użytkownika), zarządzanie ogólnym stanem widoku (ładowanie, błędy), obsługę akcji globalnych (zakończenie interakcji i nawigacja) oraz przekazywanie danych do komponentów `ProposalCard`.
*   **Główne elementy:** Komponenty `LoadingSpinner`, `ErrorMessageDisplay`, pętla `@foreach` renderująca `ProposalCard`, przycisk akcji ("Zakończ").
*   **Obsługiwane interakcje:**
    *   Inicjalne pobranie danych (`OnInitializedAsync`).
    *   Obsługa aktualizacji stanu propozycji (`HandleProposalUpdate`) po zdarzeniach z `ProposalCard`.
    *   Obsługa kliknięcia przycisku "Zakończ" (nawigacja do `/plans`).
*   **Obsługiwana walidacja:** Brak specyficznej dla tego widoku (usunięto walidację parametrów dla ponownego generowania).
*   **Typy:** `List<ProposalViewModel>`, `PlanProposalResponse` (dla API).
*   **Propsy (Parametry):** Brak specyficznych parametrów wejściowych z routingu (usunięto `NoteId`).

### ProposalCard.razor
*   **Opis komponentu:** Wyświetla pojedynczą propozycję planu w formie karty. Umożliwia edycję tytułu (inline), akceptację lub odrzucenie propozycji. Zarządza własnym stanem wizualnym (np. tryb edycji tytułu, ładowanie akcji, styl karty w zależności od statusu) i komunikuje zmiany do rodzica. Cała karta jest klikalna i przekierowuje do widoku szczegółów `/plan-proposals/{id}`.
*   **Główne elementy:** Struktura karty Bootstrap (`.card`), tytuł (`.card-title` z ikoną edycji), treść (`.card-text`), przyciski akcji ("Akceptuj", "Odrzuć", "Zapisz Tytuł", "Anuluj Edycję"). Elementy `input` do edycji tytułu, `span` do wyświetlania statusu. Cały element `.card` opakowany w element nawigacyjny lub z obsługą kliknięcia.
*   **Obsługiwane interakcje:**
    *   Kliknięcie całej karty (przekierowanie do `/plan-proposals/{Proposal.Id}`). Należy upewnić się, że kliknięcie przycisków wewnątrz karty nie propaguje tego zdarzenia.
    *   Kliknięcie ikony "Edytuj Tytuł".
    *   Kliknięcie przycisku "Zapisz Tytuł".
    *   Kliknięcie przycisku "Anuluj Edycję Tytułu".
    *   Kliknięcie przycisku "Akceptuj" (zawsze aktywne).
    *   Kliknięcie przycisku "Odrzuć" (zawsze aktywne).
*   **Obsługiwana walidacja:** Sprawdzenie długości edytowanego tytułu (<= 100 znaków) przed zapisaniem.
*   **Typy:** `ProposalViewModel` (jako parametr), `UpdatePlanDto` (dla API).
*   **Propsy (Parametry):**
    *   `[Parameter] public ProposalViewModel Proposal { get; set; }` (Wymagane)
    *   `[Parameter] public EventCallback<ProposalViewModel> OnProposalUpdated { get; set; }` (Wymagane do powiadamiania rodzica o zmianie statusu/tytułu)

## 5. Typy
*   **`PlanProposalResponse` (Odpowiedź API - Oczekiwana):**
    *   `id` (Guid): ID planu.
    *   `status` (string): Status planu ("Generated", "Accepted", "Rejected").
    *   `startDate` (DateOnly): Data rozpoczęcia.
    *   `endDate` (DateOnly): Data zakończenia.
    *   `budget` (decimal): Budżet.
    *   `title` (string): Tytuł planu.
    *   `content` (string): Pełna treść planu.
    *   `createdAt` (DateTime): Data utworzenia.
    *   `modifiedAt` (DateTime): Data modyfikacji.
    *   *(Uwaga: Zakłada się, że API `GET /api/plan-proposals` lub `GET /api/plans?status=Generated` zwróci ten typ z pełną treścią, a nie `PlanListItemDto`)*
*   **`UpdatePlanDto` (Żądanie API - PATCH /plans/{id}):**
    *   `title` (string?, opcjonalne): Nowy tytuł (max 100 znaków). Wysyłane tylko przy aktualizacji tytułu.
    *   `status` (`PlanStatus`?, opcjonalne): Nowy status (enum `Accepted` lub `Rejected`). Wysyłane tylko przy akceptacji/odrzuceniu.
    *   `content` (string?, opcjonalne): **Nie używane w tym widoku** (dotyczy aktualizacji zaakceptowanych planów).
*   **`GeneratePlanProposalRequest`:** (Nie używane w tym widoku - usunięto funkcjonalność)
*   **`ProposalViewModel` (ViewModel dla komponentów Blazor):**
    *   `Id` (Guid): ID planu.
    *   `Title` (string): Aktualny tytuł (może być modyfikowany w UI).
    *   `OriginalTitle` (string): Tytuł pobrany z API, używany do anulowania edycji.
    *   `Content` (string): Pełna treść planu.
    *   `Status` (`PlanStatus` enum): Zmapowany status planu.
    *   `StartDate` (DateOnly): Data rozpoczęcia.
    *   `EndDate` (DateOnly): Data zakończenia.
    *   `Budget` (decimal): Budżet.
    *   `IsEditingTitle` (bool): Flaga wskazująca, czy tytuł jest w trybie edycji.
    *   `IsLoadingAction` (bool): Flaga wskazująca ładowanie podczas akcji Accept/Reject/SaveTitle.
    *   `ErrorMessage` (string?): Komunikat błędu specyficzny dla karty (np. błąd zapisu tytułu).
    *   (Usunięto `CanInteract` - przyciski Akceptuj/Odrzuć są zawsze aktywne)
*   **`PlanStatus` (Enum):**
    *   `Generated`
    *   `Accepted`
    *   `Rejected`

## 6. Zarządzanie stanem
*   **`PlanProposalsView`:** Przechowuje listę `ProposalViewModel` (`ProposalsList`), stan ładowania (`isLoading`), błąd strony (`pageError`). Używa wstrzykniętych serwisów `HttpClient`, `NavigationManager`, `IToastService`. (Usunięto przechowywanie parametrów kontekstowych dla ponownego generowania). Aktualizuje `ProposalsList` na podstawie zdarzeń `OnProposalUpdated` z `ProposalCard`.
*   **`ProposalCard`:** Zarządza stanem pojedynczej propozycji (`Proposal` jako parametr), stanem edycji (`IsEditingTitle`), tymczasowym edytowanym tytułem (`editedTitle`) i stanem ładowania akcji (`IsLoadingAction`). Używa `EventCallback` (`OnProposalUpdated`) do powiadamiania rodzica o zmianach. Używa `NavigationManager` do obsługi kliknięcia karty.

Nie jest wymagany dedykowany customowy hook ani globalny store stanu dla tego widoku. Stan jest zarządzany lokalnie w komponencie strony i komponentach dzieci z komunikacją przez parametry i `EventCallback`.

## 7. Integracja API
*   **Pobranie propozycji:**
    *   Metoda: `GET`
    *   Endpoint: `/api/plan-proposals` (preferowany) lub `/api/plans?status=Generated` (lub inny mechanizm pobrania odpowiednich propozycji dla użytkownika).
    *   Odpowiedź: `Task<List<PlanProposalResponse>>`
    *   Akcja Frontend: W `PlanProposalsView.OnInitializedAsync`, wywołanie API, mapowanie odpowiedzi na `List<ProposalViewModel>`, obsługa błędów.
*   **Aktualizacja tytułu:**
    *   Metoda: `PATCH`
    *   Endpoint: `/api/plans/{proposalId}`
    *   Żądanie: `UpdatePlanDto { Title = "Nowy Tytuł" }`
    *   Odpowiedź: `Task<PlanProposalResponse>` (zaktualizowany plan)
    *   Akcja Frontend: W `ProposalCard.HandleSaveTitle`, wywołanie API, aktualizacja `ProposalViewModel`, wywołanie `OnProposalUpdated`.
*   **Akceptacja propozycji:**
    *   Metoda: `PATCH`
    *   Endpoint: `/api/plans/{proposalId}`
    *   Żądanie: `UpdatePlanDto { Status = PlanStatus.Accepted }`
    *   Odpowiedź: `Task<PlanProposalResponse>` (zaktualizowany plan)
    *   Akcja Frontend: W `ProposalCard.HandleAccept`, wywołanie API, aktualizacja `ProposalViewModel`, wywołanie `OnProposalUpdated`.
*   **Odrzucenie propozycji:**
    *   Metoda: `PATCH`
    *   Endpoint: `/api/plans/{proposalId}`
    *   Żądanie: `UpdatePlanDto { Status = PlanStatus.Rejected }`
    *   Odpowiedź: `Task<PlanProposalResponse>` (zaktualizowany plan)
    *   Akcja Frontend: W `ProposalCard.HandleReject`, wywołanie API, aktualizacja `ProposalViewModel`, wywołanie `OnProposalUpdated`.
*   **Ponowne generowanie:** (Usunięto)

## 8. Interakcje użytkownika
*   **Ładowanie widoku:** Użytkownik widzi spinner, a następnie karty propozycji lub komunikat błędu.
*   **Kliknięcie karty:** Użytkownik jest przekierowywany do `/plan-proposals/{id}`.
*   **Kliknięcie ikony ołówka (Edytuj Tytuł):** Tytuł staje się polem input, pojawiają się ikony Zapisz/Anuluj. Dostępne niezależnie od statusu.
*   **Wpisanie nowego tytułu:** Tekst w inpucie się zmienia.
*   **Kliknięcie ikony "Zapisz Tytuł":** Walidacja długości. Wywołanie API PATCH. Jeśli sukces, input znika, tytuł się aktualizuje. Jeśli błąd, toast z komunikatem.
*   **Kliknięcie ikony "Anuluj Edycję":** Input znika, tytuł wraca do `OriginalTitle`.
*   **Kliknięcie "Akceptuj":** Przycisk pokazuje spinner. Wywołanie API PATCH. Jeśli sukces, karta zmienia styl (np. zielone tło/ramka), status się aktualizuje. Jeśli błąd, toast z komunikatem, spinner znika. Przycisk zawsze dostępny.
*   **Kliknięcie "Odrzuć":** Przycisk pokazuje spinner. Wywołanie API PATCH. Jeśli sukces, karta zmienia styl (np. szare tło/ramka), status się aktualizuje. Jeśli błąd, toast z komunikatem, spinner znika. Przycisk zawsze dostępny.
*   **Kliknięcie "Generuj ponownie":** (Usunięto)
*   **Kliknięcie "Zakończ i zobacz zapisane plany":** (Jeśli widoczny) Użytkownik jest przekierowywany do widoku listy planów (`/plans`). Przycisk pojawia się po pierwszej akcji akceptacji/odrzucenia.

## 9. Warunki i walidacja
*   **Autoryzacja:** Widok (`PlanProposalsView`) oznaczony atrybutem `[Authorize]`.
*   **Dostępność akcji (Edycja/Akceptacja/Odrzucenie):** Ikona edycji oraz przyciski Akceptuj/Odrzuć są zawsze aktywne (usunięto warunek `Proposal.Status == PlanStatus.Generated`). Należy jedynie wyłączyć je podczas trwania akcji (`IsLoadingAction`).
*   **Walidacja długości tytułu:** W `ProposalCard.HandleSaveTitle`, przed wysłaniem API, sprawdzane jest, czy `editedTitle.Length <= 100`. Jeśli nie, wyświetlany jest błąd walidacji (np. pod inputem lub przez toast) i API nie jest wywoływane.
*   **Widoczność przycisku "Generuj ponownie":** (Usunięto)
*   **Widoczność przycisku "Zakończ":** W `PlanProposalsView`, przycisk jest renderowany, gdy co najmniej jedna propozycja zmieniła status z `Generated` na `Accepted` lub `Rejected` (lub po prostu `hasInteracted` jest `true`).

## 10. Obsługa błędów
*   **Błąd pobierania początkowego:** `PlanProposalsView` wyłapuje wyjątek w `OnInitializedAsync`, ustawia `pageError` i wyświetla `ErrorMessageDisplay` zamiast listy kart.
*   **Błąd API (PATCH):** W metodach obsługi akcji (`HandleSaveTitle`, `HandleAccept`, `HandleReject` w `ProposalCard`), blok `try-catch` wokół wywołania `HttpClient`. W `catch` logowanie błędu i wyświetlanie komunikatu użytkownikowi za pomocą `IToastService`. Stan ładowania (`IsLoadingAction`) jest resetowany w `finally` lub `catch`.
*   **Błąd walidacji (400 Bad Request):** Jeśli API zwraca błąd 400 z komunikatem, należy go wyświetlić użytkownikowi przez `IToastService`.
*   **Błąd serwera (5xx):** Wyświetlić ogólny komunikat błędu przez `IToastService`.
*   **Brak autoryzacji (401/403):** Powinno być obsłużone globalnie przez mechanizmy autoryzacji Blazor (przekierowanie do logowania).

## 11. Kroki implementacji
1.  **Przygotowanie:** Upewnij się, że endpoint `GET /api/plan-proposals` (lub alternatywa do pobrania właściwych propozycji) jest dostępny i zwraca `List<PlanProposalResponse>`. Potwierdź istnienie `IToastService`.
2.  **Utworzenie typów:** Zdefiniuj enum `PlanStatus` (jeśli usunięty, odtwórz go lub użyj `Data.PlanStatus`) i ViewModel `ProposalViewModel`. Usuń właściwość `CanInteract` z `ProposalViewModel`.
3.  **Modyfikacja `PlanProposalsView.razor` i `.razor.cs`:**
    *   Zmień routing na `@page "/plan-proposals"`. Usuń parametr `NoteId` i powiązaną logikę.
    *   Zmień `OnParametersSetAsync` na `OnInitializedAsync` do pobierania danych. Dostosuj logikę pobierania danych do nowego routingu (jak identyfikować, które propozycje pobrać?).
    *   Usuń przycisk "Generuj ponownie" i powiązaną logikę (`HandleGenerateAgainAsync`, `CanGenerateAgain`).
    *   Upewnij się, że przycisk "Zakończ" działa poprawnie.
4.  **Modyfikacja `ProposalCard.razor` i `.razor.cs`:**
    *   Wstrzyknij `NavigationManager`.
    *   Dodaj logikę obsługi kliknięcia całej karty, aby nawigować do `/plan-proposals/{Proposal.Id}`. Użyj `NavigationManager.NavigateTo`. Zadbaj o `@onclick:stopPropagation` na wewnętrznych przyciskach, aby uniknąć nawigacji przy ich kliknięciu.
    *   Usuń sprawdzanie `CanInteract` (lub `Proposal.Status == PlanStatus.Generated`) przy renderowaniu i obsłudze przycisków "Akceptuj", "Odrzuć" i ikony edycji tytułu. Przyciski powinny być jedynie wyłączane przez `Proposal.IsLoadingAction`.
    *   Potwierdź, że logika `HandleAccept`, `HandleReject`, `HandleSaveTitle` działa poprawnie (wywołania API PATCH, aktualizacja `ProposalViewModel`, wywołanie `OnProposalUpdated`, obsługa `IsLoadingAction` i błędów/toastów).
5.  **Styling:** Dodaj odpowiednie style CSS dla stanów karty (`card-accepted`, `card-rejected`) i ewentualnie dla trybu edycji oraz wskaźnik wizualny, że karta jest klikalna (np. `:hover` efekt).
6.  **Testowanie:** Przetestuj wszystkie przepływy użytkownika: ładowanie, kliknięcie karty (nawigacja), edycję tytułu, akceptację, odrzucenie (powinny być możliwe wielokrotnie), obsługę błędów i nawigację przyciskiem "Zakończ". Sprawdź responsywność layoutu.
7.  **(Nowy krok)** Utwórz prosty widok szczegółów `PlanProposalDetailView.razor` dostępny pod `/plan-proposals/{id:guid}`, który pobierze dane planu o podanym ID i wyświetli jego właściwości. 