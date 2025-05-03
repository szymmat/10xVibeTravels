# Architektura UI dla 10xVibeTravels

## 1. Przegląd struktury UI

Architektura interfejsu użytkownika (UI) dla aplikacji 10xVibeTravels opiera się na frameworku Blazor Server z wykorzystaniem Bootstrap do stylizacji i responsywności. Aplikacja ma strukturę Single Page Application (SPA), gdzie nawigacja odbywa się po stronie klienta bez pełnego przeładowania strony. Uwierzytelnianie i autoryzacja są zarządzane przez ASP.NET Core Identity.

Centralnym punktem dla zalogowanego użytkownika jest Dashboard, zapewniający szybki dostęp do kluczowych funkcji: tworzenia notatek, przeglądu ostatnich aktywności (notatki, plany) oraz statusu generowanych propozycji planów. Nawigacja między głównymi sekcjami (Dashboard, Notatki, Plany, Profil) odbywa się za pomocą bocznego menu.

Do interakcji z użytkownikiem wykorzystywane są modale (dla tworzenia/edycji danych, potwierdzeń), powiadomienia typu "toast" (dla informacji zwrotnych o operacjach) oraz wskaźniki ładowania (spinners). Zarządzanie stanem opiera się na wstrzykiwanych serwisach o zasięgu `Scoped`, które komunikują się z backendowym API. Kładziony jest nacisk na aktualizację tylko niezbędnych fragmentów UI po wykonaniu akcji przez użytkownika. Architektura uwzględnia podstawową responsywność (desktop/mobile).

## 2. Lista widoków

### 2.1. Widoki Tożsamości (Identity UI)
*   **Nazwa widoku:** Logowanie, Rejestracja, Zarządzanie kontem (standardowe widoki ASP.NET Core Identity)
*   **Ścieżka widoku:** `/Identity/Account/Login`, `/Identity/Account/Register`, itp.
*   **Główny cel:** Uwierzytelnianie i zarządzanie kontem użytkownika.
*   **Kluczowe informacje:** Pola formularzy logowania/rejestracji, opcje zarządzania profilem (zmiana hasła, email).
*   **Kluczowe komponenty:** Standardowe komponenty Identity UI.
*   **UX, dostępność, bezpieczeństwo:** Zapewniane przez domyślną implementację ASP.NET Core Identity.

### 2.2. Dashboard
*   **Nazwa widoku:** `Dashboard.razor`
*   **Ścieżka widoku:** `/` (dla zalogowanych użytkowników)
*   **Główny cel:** Zapewnienie centralnego punktu startowego z przeglądem aktywności i szybkim dostępem do akcji.
*   **Kluczowe informacje:**
    *   Lista 3 ostatnich notatek (Tytuły jako linki do szczegółów).
    *   Lista 3 ostatnich zaakceptowanych planów (Tytuły jako linki do szczegółów).
    *   Liczba oczekujących propozycji planów (z linkiem do widoku propozycji).
*   **Kluczowe komponenty:**
    *   Sekcje/Karty dla notatek, planów, propozycji.
    *   Przycisk "Nowa notatka" (uruchamia `QuickNoteModal`).
    *   Komponent `LoadingSpinner` (podczas ładowania danych).
    *   Komponent `EmptyState` (jeśli brak notatek/planów).
*   **UX, dostępność, bezpieczeństwo:** Wymaga autoryzacji. Zapewnia szybki przegląd i nawigację.

### 2.3. Lista Notatek
*   **Nazwa widoku:** `NotesList.razor`
*   **Ścieżka widoku:** `/notes`
*   **Główny cel:** Wyświetlanie, sortowanie i zarządzanie listą notatek użytkownika.
*   **Kluczowe informacje:**
    *   Pagowana lista notatek: Tytuł (link do `NoteDetails`), Data modyfikacji.
    *   Nagłówki kolumn umożliwiające sortowanie (Tytuł, Data utworzenia, Data modyfikacji).
*   **Kluczowe komponenty:**
    *   Tabela lub lista (`list-group`) z elementami notatek.
    *   Nagłówki z obsługą kliknięcia do sortowania (DESC -> ASC -> None -> DESC).
    *   Kontrolki paginacji ("Poprzednia", "Następna").
    *   Ikona kosza przy każdej notatce (uruchamia `DeleteConfirmationModal`).
    *   Komponent `LoadingSpinner`.
    *   Komponent `EmptyState` z przyciskiem "Utwórz pierwszą notatkę".
*   **UX, dostępność, bezpieczeństwo:** Wymaga autoryzacji. Umożliwia łatwe przeglądanie i zarządzanie notatkami.

### 2.4. Szczegóły Notatki
*   **Nazwa widoku:** `NoteDetails.razor`
*   **Ścieżka widoku:** `/notes/{id:guid}`
*   **Główny cel:** Wyświetlanie pełnej treści notatki, umożliwienie edycji, usunięcia oraz inicjowania generowania planu.
*   **Kluczowe informacje:**
    *   Tytuł notatki.
    *   Treść notatki.
    *   Data utworzenia, Data modyfikacji.
*   **Kluczowe komponenty:**
    *   Wyświetlanie statyczne tytułu i treści.
    *   Przycisk "Edytuj Notatkę" (uruchamia `EditNoteModal`).
    *   Przycisk "Usuń Notatkę" (uruchamia `DeleteConfirmationModal`).
    *   Przycisk "Generuj Plan" (uruchamia `GeneratePlanModal`).
    *   Komponent `LoadingSpinner` (podczas ładowania danych).
*   **UX, dostępność, bezpieczeństwo:** Wymaga autoryzacji. Użytkownik może modyfikować tylko swoje notatki (logika sprawdzana w backendzie i/lub serwisie).

### 2.5. Widok Propozycji Planów
*   **Nazwa widoku:** `PlanProposalsView.razor`
*   **Ścieżka widoku:** `/plan-proposals` (lub `/plans?status=Generated` - do ustalenia preferowana ścieżka)
*   **Główny cel:** Porównanie i zarządzanie (akceptacja, odrzucenie, edycja tytułu) trzema wygenerowanymi przez AI propozycjami planów.
*   **Kluczowe informacje:**
    *   Trzy karty propozycji wyświetlane obok siebie (desktop) lub jedna pod drugą (mobile).
    *   Każda karta zawiera: Tytuł (z możliwością edycji inline), Pełna treść planu, Przyciski akcji ("Akceptuj", "Odrzuć", ikona "Edytuj Tytuł").
    *   Wizualne oznaczenie statusu po akceptacji/odrzuceniu.
*   **Kluczowe komponenty:**
    *   Komponent `ProposalCard.razor` (dla każdej propozycji):
        *   Wyświetlanie tytułu i treści.
        *   Mechanizm edycji inline tytułu (ikona ołówka -> ikony zapisu/anulowania, input tekstowy). Obsługa API (`PATCH /plans/{id}`) i błędów (tooltip).
        *   Przyciski "Akceptuj", "Odrzuć" wywołujące API (`PATCH /plans/{id}`).
        *   Logika zmiany wyglądu (tło, etykieta statusu, deaktywacja przycisków) po akcji.
    *   Przycisk "Generuj ponownie" (widoczny tylko gdy wszystkie 3 propozycje są odrzucone). Wywołuje API (`POST /plan-proposals`).
    *   Układ siatki Bootstrap (3 kolumny / 1 kolumna).
    *   Komponent `LoadingSpinner` (podczas ładowania propozycji lub regeneracji).
    *   Komponent `ToastService` do wyświetlania informacji zwrotnych.
*   **UX, dostępność, bezpieczeństwo:** Wymaga autoryzacji. Umożliwia łatwe porównanie i podjęcie decyzji. Edycja inline tytułu usprawnia personalizację. Stan jest odzwierciedlany wizualnie bez przeładowania.

### 2.6. Lista Planów
*   **Nazwa widoku:** `PlansList.razor`
*   **Ścieżka widoku:** `/plans`
*   **Główny cel:** Wyświetlanie, filtrowanie, sortowanie i zarządzanie listą zaakceptowanych lub odrzuconych planów użytkownika.
*   **Kluczowe informacje:**
    *   Pagowana lista planów: Tytuł (link do `PlanDetails`), Status (etykieta `badge`), Data modyfikacji, (opcjonalnie: Daty podróży, Budżet).
    *   Nagłówki kolumn umożliwiające sortowanie (Tytuł, Data utworzenia, Data modyfikacji).
*   **Kluczowe komponenty:**
    *   Dropdown do filtrowania wg statusu ("Zaakceptowane" - domyślny, "Odrzucone").
    *   Tabela lub lista (`list-group`) z elementami planów.
    *   Etykiety statusu (`Bootstrap badges`).
    *   Nagłówki z obsługą kliknięcia do sortowania.
    *   Kontrolki paginacji.
    *   Ikona kosza przy każdym planie (uruchamia `DeleteConfirmationModal`).
    *   Komponent `LoadingSpinner`.
    *   Komponent `EmptyState` z odpowiednim komunikatem (np. "Brak zapisanych planów").
*   **UX, dostępność, bezpieczeństwo:** Wymaga autoryzacji. Filtracja i sortowanie ułatwiają zarządzanie planami.

### 2.7. Szczegóły Planu
*   **Nazwa widoku:** `PlanDetails.razor`
*   **Ścieżka widoku:** `/plans/{id:guid}`
*   **Główny cel:** Wyświetlanie pełnych szczegółów planu (zaakceptowanego, odrzuconego lub nawet generowanego), umożliwienie edycji treści (dla zaakceptowanych) i usunięcia.
*   **Kluczowe informacje:**
    *   Tytuł planu.
    *   Treść planu (z możliwością edycji jeśli status='Accepted').
    *   Status planu (etykieta `badge`).
    *   Data rozpoczęcia, Data zakończenia.
    *   Budżet.
    *   Data utworzenia, Data modyfikacji.
*   **Kluczowe komponenty:**
    *   Wyświetlanie statyczne danych planu.
    *   Etykieta statusu (`Bootstrap badge`).
    *   Przycisk "Edytuj Treść" (widoczny tylko dla `Status='Accepted'`). Kliknięcie zamienia statyczny tekst treści na `InputTextArea` i pokazuje przycisk "Zapisz Zmiany".
    *   Przycisk "Zapisz Zmiany" (widoczny podczas edycji treści). Wywołuje API (`PATCH /plans/{id}`).
    *   Przycisk "Usuń Plan" (uruchamia `DeleteConfirmationModal`).
    *   Komponent `LoadingSpinner` (podczas ładowania danych lub zapisywania zmian).
    *   Komponent `ToastService` do informacji zwrotnych.
*   **UX, dostępność, bezpieczeństwo:** Wymaga autoryzacji. Użytkownik może edytować treść tylko zaakceptowanych planów i usuwać tylko swoje plany.

### 2.8. Profil Użytkownika
*   **Nazwa widoku:** `Profile.razor`
*   **Ścieżka widoku:** `/profile`
*   **Główny cel:** Umożliwienie użytkownikowi przeglądania i edycji swoich preferencji podróżniczych (Budżet, Styl, Intensywność, Zainteresowania).
*   **Kluczowe informacje:**
    *   Aktualne wartości preferencji.
    *   Opcje do wyboru dla Stylu, Intensywności, Zainteresowań (pobierane z API słowników).
*   **Kluczowe komponenty:**
    *   Formularz `EditForm`.
    *   `InputNumber` dla Budżetu.
    *   `InputRadioGroup` dla Stylu Podróży (opcje z `/travel-styles`).
    *   `InputRadioGroup` dla Intensywności (opcje z `/intensities`).
    *   Komponent Multi-select dropdown dla Zainteresowań (opcje z `/interests`).
    *   Przyciski "Zapisz zmiany" (wywołujące `PATCH /profile` i `PUT /profile/interests`).
    *   Komponent `LoadingSpinner` (podczas ładowania danych profilu i słowników).
    *   Komponent `ToastService` do informacji zwrotnych.
*   **UX, dostępność, bezpieczeństwo:** Wymaga autoryzacji. Umożliwia personalizację preferencji wpływających na generowanie planów.

## 3. Mapa podróży użytkownika

Główny przepływ użytkownika (Happy Path - Generowanie i akceptacja planu):

1.  **Logowanie:** Użytkownik wchodzi na stronę, jest przekierowany do `/Identity/Account/Login`, loguje się.
2.  **Dashboard:** Użytkownik jest przekierowany na `/`. Widzi przegląd, klika "Nowa notatka".
3.  **Tworzenie Notatki:** Otwiera się `QuickNoteModal`. Użytkownik wpisuje tytuł i treść, klika "Zapisz". Modal się zamyka, pojawia się toast "Notatka zapisana", lista na Dashboardzie się odświeża.
4.  **Nawigacja do Notatki:** Użytkownik klika tytuł nowej notatki na Dashboardzie (lub nawiguje przez `/notes`).
5.  **Szczegóły Notatki:** Użytkownik jest na `/notes/{id}`. Przegląda notatkę, klika "Generuj Plan".
6.  **Konfiguracja Generowania:** Otwiera się `GeneratePlanModal`. Użytkownik wybiera daty (Start, End), opcjonalnie podaje budżet, klika "Generuj". Modal się zamyka, pojawia się spinner/wskaźnik ładowania.
7.  **Widok Propozycji:** Po otrzymaniu odpowiedzi z API (`POST /plan-proposals`), użytkownik jest automatycznie przekierowany do `/plan-proposals` (lub `/plans?status=Generated`). Widzi 3 karty propozycji.
8.  **Interakcja z Propozycjami:**
    *   Użytkownik edytuje tytuł jednej propozycji (inline edit).
    *   Użytkownik klika "Akceptuj" na wybranej propozycji. Karta zmienia wygląd (tło, status, przyciski nieaktywne), pojawia się toast "Plan zaakceptowany".
    *   Użytkownik klika "Odrzuć" na pozostałych dwóch. Karty zmieniają wygląd, pojawiają się toasty.
9.  **Nawigacja do Planów:** Użytkownik klika "Moje Plany" w menu bocznym.
10. **Lista Planów:** Użytkownik jest na `/plans`. Widzi zaakceptowany plan na liście (filtr domyślnie "Zaakceptowane"). Może przełączyć filtr na "Odrzucone", aby zobaczyć odrzucone propozycje.
11. **Nawigacja do Szczegółów Planu:** Użytkownik klika tytuł zaakceptowanego planu.
12. **Szczegóły Planu:** Użytkownik jest na `/plans/{id}`. Widzi szczegóły zaakceptowanego planu. Klika "Edytuj Treść".
13. **Edycja Treści Planu:** Pole treści zamienia się w `InputTextArea`, pojawia się przycisk "Zapisz Zmiany". Użytkownik modyfikuje treść, klika "Zapisz Zmiany". Pojawia się spinner, treść się aktualizuje, pojawia się toast "Zmiany zapisane".
14. **Usuwanie Planu (Opcjonalnie):** Użytkownik wraca do `/plans`, klika ikonę kosza przy planie. Otwiera się `DeleteConfirmationModal`. Klika "Potwierdź". Plan znika z listy, pojawia się toast "Plan usunięty".

Inne przepływy: Edycja profilu, edycja notatki, regenerowanie propozycji, przeglądanie list z paginacją/sortowaniem.

## 4. Układ i struktura nawigacji

*   **Główny Układ:** Standardowy układ z bocznym menu nawigacyjnym (sidebar) i obszarem treści. Układ wykorzystuje Bootstrap do responsywności (menu może być zwijane na mniejszych ekranach).
*   **Menu Boczne (`NavMenu.razor`):** Zawiera linki do głównych sekcji aplikacji dla zalogowanego użytkownika:
    *   Dashboard (`/`)
    *   Notatki (`/notes`)
    *   Oczekujące Propozycje (`/plan-proposals` lub `/plans?status=Generated`)
    *   Moje Plany (`/plans`)
    *   Profil (`/profile`)
    *   Opcjonalnie: Wyloguj.
*   **Nawigacja Kontekstowa:**
    *   Linki w listach (np. tytuły notatek/planów) prowadzą do odpowiednich widoków szczegółów.
    *   Przyciski w widokach szczegółów lub listach inicjują akcje (np. "Edytuj", "Usuń", "Generuj Plan"), często otwierając modale.
    *   Dropdown w widoku `PlansList` służy do filtrowania listy wg statusu.

## 5. Kluczowe komponenty

*   **`LoadingSpinner.razor`:** Wyświetla wskaźnik ładowania. Używany podczas pobierania danych list, ładowania szczegółów, wykonywania długotrwałych akcji API (np. generowanie planu, zapis). Może być wyświetlany w miejscu treści lub jako overlay na przycisku.
*   **`EmptyState.razor`:** Komponent wyświetlany, gdy lista (notatek, planów, propozycji) jest pusta. Zawiera konfigurowalny komunikat i opcjonalny przycisk Call To Action (np. "Utwórz pierwszą notatkę").
*   **`ToastService` / `ToastContainer.razor`:** Globalny serwis do zarządzania powiadomieniami "toast" (sukces, błąd, informacja) i komponent do ich wyświetlania (zwykle w rogu ekranu).
*   **`DeleteConfirmationModal.razor`:** Reużywalny modal wyświetlający prośbę o potwierdzenie przed wykonaniem operacji usunięcia (notatki, planu). Przyjmuje tytuł/treść wiadomości i callback do wykonania po potwierdzeniu.
*   **`QuickNoteModal.razor`:** Modal z formularzem (`EditForm`, `InputText`, `InputTextArea`, `ValidationSummary`) do szybkiego tworzenia nowej notatki (`POST /notes`). Uruchamiany z Dashboardu.
*   **`EditNoteModal.razor`:** Modal analogiczny do `QuickNoteModal`, ale do edycji istniejącej notatki (`PUT /notes/{id}`). Ładuje dane notatki do formularza. Uruchamiany z `NoteDetails`.
*   **`GeneratePlanModal.razor`:** Modal z formularzem (`EditForm`, `InputDate` x2, `InputNumber`, `ValidationSummary`) do zbierania parametrów (startDate, endDate, opcjonalny budget) przed wywołaniem generowania planu (`POST /plan-proposals`). Zawiera walidację `EndDate >= StartDate`. Uruchamiany z `NoteDetails`.
*   **`ProposalCard.razor`:** Komponent reprezentujący pojedynczą propozycję planu w `PlanProposalsView`. Zawiera logikę wyświetlania danych, edycji inline tytułu, obsługi przycisków akcji (Accept/Reject) i aktualizacji wizualnej stanu karty.
*   **Sortable Header Component:** (Potencjalnie reużywalny komponent lub logika w `NotesList`/`PlansList`) Obsługuje kliknięcia nagłówków tabeli/listy do zmiany kryteriów sortowania (`sortBy`, `sortDirection`) i wizualnego wskazania aktywnego sortowania.
*   **Pagination Component:** Kontrolki do nawigacji między stronami wyników dla list (`NotesList`, `PlansList`). 