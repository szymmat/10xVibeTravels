# Plan implementacji widoku Profile.razor

## 1. Przegląd
Widok **Profile.razor** służy do wyświetlania i edycji preferencji podróżniczych zalogowanego użytkownika: budżetu, stylu podróży, intensywności oraz zainteresowań. Zapewnia ładowanie danych z API, walidację, feedback (spinner i toasty) oraz obsługę błędów.

## 2. Routing widoku
Ścieżka: `/profile` (w pliku `App.razor` lub na routerze Blazor powinna być zadeklarowana np.: `<RouteView RouteData="..." DefaultLayout="..." Path="/profile" />`).

## 3. Struktura komponentów
```
ProfilePage (Profile.razor)
├─ LoadingSpinner (pokazuje się gdy isLoading)
├─ PromptToCompleteProfile (conditional dla US-003)
└─ ProfileForm (komponent edycji formularza)
   ├─ BudgetInput (InputNumber<decimal?>)
   ├─ TravelStyleRadioGroup (InputRadioGroup<Guid?>)
   ├─ IntensityRadioGroup (InputRadioGroup<Guid?>)
   ├─ InterestsMultiSelect (custom InputBase<List<Guid>>)
   └─ SaveButtons (przyciski Zapisz preferencje i Zapisz zainteresowania)
```

## 4. Szczegóły komponentów

### ProfilePage
- Opis: kontener ładowania danych, zarządza stanem `isLoading`, `showPrompt` i danymi formularza.
- Główne elementy: `@if (isLoading) LoadingSpinner`, `PromptToCompleteProfile`, `ProfileForm`.
- Obsługiwane interakcje: inicjalne `OnInitializedAsync()` wywołuje `LoadData()`.

### PromptToCompleteProfile
- Opis: baner/modal zachęcający do wypełnienia profilu (US-003).
- Główne elementy: tekst, przycisk "Wypełnij profil", ikona zamknięcia.
- Obsługiwane interakcje: klik "Wypełnij profil" → przewija do formy; klik zamknij → `showPrompt=false`.

### ProfileForm
- Opis: `EditForm` z modelem `ProfileViewModel`.
- Główne elementy:
  - `<InputNumber @bind-Value="model.Budget" />`
  - `<InputRadioGroup @bind-Value="model.TravelStyleId">` z opcjami 
  - `<InputRadioGroup @bind-Value="model.IntensityId">`
  - `<InterestsMultiSelect @bind-Value="model.Interests" Options="interestsOptions" />`
- Obsługiwane zdarzenia: `OnValidSubmit="HandleValidSubmit"`.
- Walidacja:
  - Budget >= 0 (atrybuty lub ręcznie w `EditContext`).
  - model.Interests != null (może być pusta lista).
- Typy:
  - Model: `ProfileViewModel { decimal? Budget; Guid? TravelStyleId; Guid? IntensityId; List<Guid> Interests; }`
  - Opcje: `LookupOption { Guid Id; string Name; }`
- Propsy: `ProfileViewModel model`, `List<LookupOption> travelStyles`, `intensities`, `interestsOptions`, `EventCallback<ProfileViewModel> OnSubmit`.

### InterestsMultiSelect
- Opis: wielokrotny wybór `Guid` dla zainteresowań.
- Elementy: `<select multiple>` opakowane w custom `InputBase<List<Guid>>`.
- Zdarzenia: `OnChange` aktualizuje listę `List<Guid>`.
- Walidacja: brak duplikatów, wymagany typ GUID.
- Propsy: `List<LookupOption> Options`, `List<Guid> Value`, `EventCallback<List<Guid>> ValueChanged`.

### SaveButtons
- Opis: zawiera przyciski:
  1. Zapisz preferencje (PATCH `/profile`)
  2. Zapisz zainteresowania (PUT `/profile/interests`)
- Elementy: `<button>` ze spinnerem (disabled podczas zapisu lub gdy formularz niepoprawny).
- Obsługa: wywołuje przekazane callbacki.

## 5. Typy
```csharp
public class ProfileViewModel {
    public decimal? Budget { get; set; }
    public Guid? TravelStyleId { get; set; }
    public Guid? IntensityId { get; set; }
    public List<Guid> Interests { get; set; } = new();
}

public class LookupOption {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
```

## 6. Zarządzanie stanem
- W `ProfilePage`:
  - `bool isLoading, isSavingPrefs, isSavingInterests;`
  - `ProfileViewModel model;`
  - `List<LookupOption> travelStyles, intensities, interestsOptions;`
  - `bool showPrompt;`
- Wykorzystać `OnInitializedAsync()` do `LoadData()`:
  1. Pobranie słowników (3 żądania równolegle).
  2. GET `/profile` → `model` oraz flaga `isNewProfile`.
  3. `showPrompt = isNewProfile`.

## 7. Integracja API
- GET `/profile` → zwraca `UserProfileDto` → mapowanie do `ProfileViewModel` (pola ID z obiektów `LookupDto`).
- PATCH `/profile` → body `{ budget, travelStyleId, intensityId }` → po sukcesie update `model` i toast.
- PUT `/profile/interests` → body `{ interestIds: model.Interests }` → update lista interesów i toast.

## 8. Interakcje użytkownika
1. Wejście na `/profile` → spinner do czasu `LoadData()` → render formy i/lub prompt.
2. Użytkownik edytuje pole budżetu, wybiera styl/intensywność, zaznacza zainteresowania.
3. Klik "Zapisz preferencje" → spinner przy przycisku → wywołanie PATCH → toast on success/error.
4. Klik "Zapisz zainteresowania" → spinner → PUT → toast.
5. Zamknięcie promptu → wyłącza `showPrompt` (bez ponownego pokazania).

## 9. Warunki i walidacja
- Budget: >= 0 (InputNumber + walidacja w `EditContext`).
- TravelStyleId, IntensityId: opcjonalne, pozwalamy null (radio zostaje odznaczone).
- Interests: lista GUID (nie ma obowiązku mieć elementów).
- Formularz `ProfileForm` powinien blokować przycisk zapisz jeśli `!EditContext.Validate()`.

## 10. Obsługa błędów
- Błąd ładowania: `catch` w `LoadData()` → `ToastService.ShowError("Błąd ładowania profilu...")` oraz ewentualny fallback.
- Błąd zapisu preferencji/interesów: `ToastService.ShowError(...)`, przycisk odblokowany.
- Błędy walidacji 400: wyświetlić komunikat z treści odpowiedzi, rozbić na pola (opcjonalnie).

## 11. Kroki implementacji
1. Utworzyć plik `Profile.razor` w folderze `Pages` z routowaniem `/profile`.
2. Zaimplementować `ProfilePage`:
   - Zdefiniować stany i metodę `LoadData()`.
   - W `OnInitializedAsync()` wywołać `LoadData()`.
3. Stworzyć komponent `PromptToCompleteProfile.razor`.
4. Stworzyć `ProfileForm.razor` z modelem `ProfileViewModel` i `EditForm`.
5. Stworzyć `InterestsMultiSelect.razor` dziedziczący po `InputBase<List<Guid>>`.
6. Dodać serwis `ProfileApiService` w DI do wywołań GET/PATCH/PUT.
7. Zaimplementować integrację w `ProfilePage` i obsługę `ToastService`.
8. Dodać walidację w `ProfileForm` za pomocą `FluentValidation` lub ręcznej.
9. Przetestować scenariusze: ładowanie, zapis, błędy, prompt.
10. Zaktualizować `NavMenu.razor` lub nawigację, aby link `/profile` był widoczny.

---
*Plan zgodny z PRD, user stories i dostarczonym stackiem technologicznym.* 