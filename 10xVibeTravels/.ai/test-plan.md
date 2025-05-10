# Plan Testów dla Projektu 10xVibeTravels

## 1. Wprowadzenie i Cele Testowania

### 1.1. Wprowadzenie

Niniejszy dokument przedstawia plan testów dla aplikacji 10xVibeTravels, platformy do generowania spersonalizowanych planów podróży z wykorzystaniem sztucznej inteligencji. Plan obejmuje strategię, zakres, zasoby i harmonogram działań testowych mających na celu zapewnienie wysokiej jakości produktu końcowego. Projekt oparty jest o technologie .NET (ASP.NET Core, Blazor Server, Entity Framework Core) oraz integruje się z zewnętrzną usługą AI (Openrouter.ai).

### 1.2. Cele Testowania

Główne cele procesu testowania to:

*   Weryfikacja, czy aplikacja spełnia zdefiniowane wymagania funkcjonalne i niefunkcjonalne.
*   Identyfikacja i raportowanie defektów oprogramowania.
*   Zapewnienie stabilności, niezawodności i bezpieczeństwa aplikacji.
*   Ocena jakości generowanych planów podróży.
*   Minimalizacja ryzyka wystąpienia błędów w środowisku produkcyjnym.
*   Zapewnienie pozytywnego doświadczenia użytkownika (UX).
*   Potwierdzenie gotowości aplikacji do wdrożenia.

## 2. Zakres Testów

### 2.1. Funkcjonalności objęte testami:

*   **Zarządzanie Profilem Użytkownika:**
    *   Tworzenie i edycja profilu.
    *   Definiowanie preferencji podróżniczych (style, zainteresowania, intensywność).
*   **Generowanie Planów Podróży:**
    *   Tworzenie propozycji planów na podstawie preferencji użytkownika.
    *   Interakcja z usługą Openrouter.ai.
    *   Prezentacja wygenerowanych planów.
*   **Zarządzanie Planami Podróży:**
    *   Przeglądanie listy planów (z paginacją i filtrowaniem).
    *   Przeglądanie szczegółów planu.
    *   Aktualizacja statusu planu.
    *   Modyfikacja planów.
    *   Usuwanie planów.
*   **Zarządzanie Notatkami do Planów:**
    *   Tworzenie, edycja, usuwanie notatek przypisanych do planów.
    *   Kontrola dostępu do notatek (tylko właściciel).
*   **Słowniki Danych:**
    *   Pobieranie list stylów podróży, zainteresowań, intensywności.
*   **Autentykacja i Autoryzacja:**
    *   Logowanie i wylogowywanie użytkowników.
    *   Zabezpieczenie dostępu do poszczególnych funkcjonalności i danych.
*   **Interfejs Użytkownika (Blazor Components):**
    *   Poprawność renderowania komponentów.
    *   Interaktywność elementów UI.
    *   Nawigacja w aplikacji.
    *   Responsywność na różnych urządzeniach.

### 2.2. Funkcjonalności wyłączone z testów (jeśli dotyczy):

*   Na chwilę obecną zakłada się pełne pokrycie testowe kluczowych modułów. Ewentualne wyłączenia zostaną udokumentowane w przyszłości, jeśli zajdzie taka potrzeba.

## 3. Typy Testów do Przeprowadzenia

*   **Testy Jednostkowe (Unit Tests):**
    *   **Cel:** Weryfikacja poprawności działania poszczególnych, izolowanych komponentów (metody w serwisach, walidatory, logika w kontrolerach).
    *   **Zakres:** Głównie warstwa `Services`, `Validators`, `Controllers` oraz logika pomocnicza. Mockowanie zależności (np. `DbContext`, `IOpenRouterService`).
*   **Testy Integracyjne (Integration Tests):**
    *   **Cel:** Weryfikacja współpracy pomiędzy komponentami systemu oraz integracji z zewnętrznymi usługami (np. baza danych, Openrouter.ai - z użyciem mocków lub dedykowanego środowiska testowego API).
    *   **Zakres:**
        *   Interakcja Kontroler -> Serwis -> Baza Danych.
        *   Interakcja `PlanGenerationService` -> `OpenRouterService` (z mockiem OpenRouter API).
        *   Poprawność działania zapytań Entity Framework Core.
*   **Testy API (API Tests):**
    *   **Cel:** Weryfikacja poprawności działania endpointów API (żądania, odpowiedzi, kody statusu, obsługa błędów, autoryzacja).
    *   **Zakres:** Wszystkie endpointy w `Controllers`.
*   **Testy Komponentów UI (Blazor Component Tests):**
    *   **Cel:** Weryfikacja renderowania, logiki i interakcji poszczególnych komponentów Blazor.
    *   **Zakres:** Kluczowe komponenty w katalogu `Components` (np. formularze, listy, elementy nawigacyjne).
*   **Testy End-to-End (E2E Tests):**
    *   **Cel:** Symulacja rzeczywistych scenariuszy użytkownika, weryfikacja przepływów w całej aplikacji od interfejsu użytkownika po bazę danych.
    *   **Zakres:** Główne ścieżki użytkownika, np. rejestracja -> tworzenie profilu -> generowanie planu -> przeglądanie planu.
*   **Testy Wydajnościowe (Performance Tests):**
    *   **Cel:** Ocena szybkości odpowiedzi aplikacji, zużycia zasobów pod obciążeniem.
    *   **Zakres:** Kluczowe operacje: generowanie planu, ładowanie list planów, zapytania do bazy danych.
*   **Testy Bezpieczeństwa (Security Tests):**
    *   **Cel:** Identyfikacja podatności aplikacji, weryfikacja mechanizmów autentykacji i autoryzacji.
    *   **Zakres:** Ochrona przed OWASP Top 10 (np. SQL Injection, XSS), kontrola dostępu do zasobów.
*   **Testy Użyteczności (Usability Tests):**
    *   **Cel:** Ocena łatwości obsługi, intuicyjności interfejsu i ogólnego doświadczenia użytkownika.
    *   **Zakres:** Przeprowadzane manualnie, potencjalnie z udziałem użytkowników.
*   **Testy Akceptacyjne Użytkownika (UAT - User Acceptance Tests):**
    *   **Cel:** Potwierdzenie przez klienta/użytkowników końcowych, że aplikacja spełnia ich oczekiwania i potrzeby.

## 4. Scenariusze Testowe dla Kluczowych Funkcjonalności

(Przykładowe scenariusze, lista będzie rozwijana w miarę rozwoju projektu)

### 4.1. Profil Użytkownika

| ID Scenariusza | Opis Scenariusza                                                                 | Oczekiwany Rezultat                                                                    | Priorytet | Typ Testu           |
|----------------|---------------------------------------------------------------------------------|----------------------------------------------------------------------------------------|-----------|---------------------|
| TC_PROF_001    | Użytkownik tworzy nowy profil, podając wszystkie wymagane dane.                  | Profil zostaje pomyślnie zapisany w bazie danych. Użytkownik widzi potwierdzenie.      | Wysoki    | E2E, Integracyjny   |
| TC_PROF_002    | Użytkownik edytuje istniejący profil, zmieniając preferencje podróży.            | Zmiany są zapisywane. Nowe preferencje są uwzględniane przy generowaniu planów.        | Wysoki    | E2E, Integracyjny   |
| TC_PROF_003    | Użytkownik próbuje zapisać profil z niepoprawnymi danymi (np. brak nazwy).        | Wyświetlany jest komunikat o błędzie walidacji. Profil nie zostaje zapisany.            | Średni    | E2E, Jednostkowy    |
| TC_PROF_004    | Użytkownik ustawia swoje zainteresowania.                                       | Zainteresowania zostają poprawnie zapisane i powiązane z profilem użytkownika.           | Wysoki    | E2E, Integracyjny   |

### 4.2. Generowanie Planu Podróży

| ID Scenariusza | Opis Scenariusza                                                                                 | Oczekiwany Rezultat                                                                                                | Priorytet | Typ Testu                  |
|----------------|--------------------------------------------------------------------------------------------------|--------------------------------------------------------------------------------------------------------------------|-----------|----------------------------|
| TC_PLAN_GEN_001| Użytkownik z wypełnionym profilem generuje propozycję planu.                                       | Aplikacja komunikuje się z OpenRouter.ai. Wygenerowany plan jest prezentowany użytkownikowi. Plan zostaje zapisany. | Krytyczny | E2E, Integracyjny          |
| TC_PLAN_GEN_002| Użytkownik generuje plan, gdy serwis OpenRouter.ai jest niedostępny lub zwraca błąd.               | Użytkownik otrzymuje stosowny komunikat o błędzie. Aplikacja nie ulega awarii.                                      | Wysoki    | E2E, Integracyjny (mock) |
| TC_PLAN_GEN_003| Weryfikacja jakości i trafności generowanego planu dla różnych zestawów preferencji użytkownika. | Plan jest spójny z preferencjami (styl, zainteresowania, intensywność).                                              | Wysoki    | Manualny, E2E              |
| TC_PLAN_GEN_004| Generowanie planu z uwzględnieniem limitów API OpenRouter.                                       | Aplikacja poprawnie obsługuje limity, informuje użytkownika lub ogranicza funkcjonalność.                          | Średni    | Integracyjny (mock)      |

### 4.3. Zarządzanie Planami

| ID Scenariusza | Opis Scenariusza                                                        | Oczekiwany Rezultat                                                              | Priorytet | Typ Testu         |
|----------------|-------------------------------------------------------------------------|----------------------------------------------------------------------------------|-----------|-------------------|
| TC_PLAN_MAN_001| Użytkownik przegląda listę swoich planów z zastosowaniem paginacji.       | Lista planów jest poprawnie wyświetlana, paginacja działa zgodnie z oczekiwaniami. | Wysoki    | E2E, Integracyjny |
| TC_PLAN_MAN_002| Użytkownik filtruje listę planów po statusie.                             | Wyświetlane są tylko plany o zadanym statusie.                                     | Wysoki    | E2E, Integracyjny |
| TC_PLAN_MAN_003| Użytkownik modyfikuje istniejący plan.                                    | Zmiany w planie są poprawnie zapisywane w bazie danych.                            | Wysoki    | E2E, Integracyjny |
| TC_PLAN_MAN_004| Użytkownik usuwa plan.                                                    | Plan zostaje usunięty z bazy danych i nie jest już widoczny na liście.             | Średni    | E2E, Integracyjny |

### 4.4. Zarządzanie Notatkami

| ID Scenariusza | Opis Scenariusza                                                          | Oczekiwany Rezultat                                                                       | Priorytet | Typ Testu         |
|----------------|---------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|-----------|-------------------|
| TC_NOTE_001    | Użytkownik dodaje notatkę do swojego planu.                                | Notatka zostaje zapisana i jest widoczna przy szczegółach planu.                            | Wysoki    | E2E, Integracyjny |
| TC_NOTE_002    | Użytkownik próbuje wyświetlić/edytować notatkę innego użytkownika.         | Odmowa dostępu. Wyświetlenie komunikatu o braku uprawnień (`NoteAccessDeniedException`).    | Krytyczny | E2E, Bezpieczeństwa |
| TC_NOTE_003    | Użytkownik edytuje istniejącą notatkę.                                    | Zmiany w notatce są zapisywane.                                                           | Wysoki    | E2E, Integracyjny |
| TC_NOTE_004    | Użytkownik usuwa notatkę.                                                 | Notatka zostaje usunięta.                                                                 | Średni    | E2E, Integracyjny |
| TC_NOTE_005    | Próba dodania notatki do nieistniejącego planu.                           | Aplikacja obsługuje błąd (np. zwraca odpowiedni status HTTP, `PlanNotFoundException`).       | Średni    | API, Integracyjny |

## 5. Środowisko Testowe

*   **Środowisko Deweloperskie:** Lokalne maszyny deweloperów. Używane do testów jednostkowych i wczesnych testów integracyjnych. Baza danych: MS SQL Server LocalDB lub SQLite in-memory.
*   **Środowisko Testowe (Staging):** Oddzielna instancja aplikacji wdrożona na serwerze (np. DigitalOcean w kontenerze Docker), z własną bazą danych (MS SQL Server, może być kopią produkcyjnej zanonimizowaną lub dedykowaną testową). Konfiguracja zbliżona do produkcyjnej. Używane do testów integracyjnych, API, E2E, wydajnościowych, UAT. Zintegrowane z CI/CD (GitHub Actions).
*   **Środowisko Produkcyjne:** Dostępne tylko do smoke testów po wdrożeniu.

Konfiguracja środowiska testowego będzie obejmować:
*   Dedykowaną bazę danych MS SQL Server.
*   Skonfigurowany dostęp do (lub mock) Openrouter.ai API (z osobnym kluczem API dla testów, z ustawionymi niskimi limitami finansowymi i użycia).
*   Narzędzia do monitorowania i logowania (np. integracja z Application Insights lub podobnym).

## 6. Narzędzia do Testowania

*   **Testy Jednostkowe (.NET):** xUnit (preferowane), NUnit lub MSTest. Biblioteki do mockowania: Moq, NSubstitute.
*   **Testy Komponentów Blazor:** bUnit.
*   **Testy Integracyjne (.NET):** xUnit/NUnit z wykorzystaniem `WebApplicationFactory` dla testów API, Entity Framework Core In-Memory Provider (dla szybszych testów) lub Testcontainers (dla testów na rzeczywistej instancji MS SQL Server w Dockerze).
*   **Testy API:** Postman (manualne eksploracyjne), RestSharp/HttpClient w C# (automatyczne w ramach testów integracyjnych lub dedykowanych projektów testowych API).
*   **Testy E2E:** Playwright (preferowane ze względu na wsparcie dla C# i nowoczesne podejście) lub Selenium.
*   **Testy Wydajnościowe:** k6 (ze względu na elastyczność i możliwość pisania skryptów w JavaScript/TypeScript) lub Apache JMeter.
*   **Zarządzanie Testami i Błędami:** GitHub Issues z odpowiednimi etykietami (np. `bug`, `enhancement`, `test-case`) i projektami GitHub do śledzenia postępów.
*   **CI/CD:** GitHub Actions (do automatycznego uruchamiania wszystkich typów zautomatyzowanych testów przy każdym pushu/pull requeście).

## 7. Harmonogram Testów

(Harmonogram jest poglądowy i będzie dostosowywany do postępów projektu i metodyki Agile/Scrum)

*   **Sprint 0 / Faza Inicjalna:**
    *   Planowanie testów, konfiguracja narzędzi i środowisk testowych. (Zakończone)
*   **Każdy Sprint Deweloperski:**
    *   Testy jednostkowe: Pisane przez deweloperów równolegle z kodem.
    *   Testy integracyjne: Pisane przez deweloperów/QA dla nowych funkcjonalności i integracji.
    *   Testy komponentów UI (Blazor): Pisane przez deweloperów/QA.
    *   Testy API: Weryfikacja nowych/zmienionych endpointów.
    *   Testy eksploracyjne: Wykonywane przez QA na zakończenie implementacji historyjek użytkownika.
*   **Faza Stabilizacji / Przed Wydaniem:**
    *   Pełne cykle testów regresji (automatyczne i manualne).
    *   Testy E2E obejmujące kluczowe przepływy.
    *   Testy wydajnościowe (dla zidentyfikowanych krytycznych ścieżek).
    *   Testy bezpieczeństwa (podstawowe skanowanie, przegląd kodu pod kątem podatności).
    *   Testy Użyteczności.
*   **Testy Akceptacyjne Użytkownika (UAT):**
    *   Po zakończeniu fazy stabilizacji, przed wdrożeniem na produkcję.
*   **Po Wdrożeniu (Release):**
    *   Smoke testy na środowisku produkcyjnym.

## 8. Kryteria Akceptacji Testów

### 8.1. Kryteria Wejścia (Rozpoczęcia danego etapu/typu testów)

*   Dostępna dokumentacja wymagań (historyjki użytkownika, kryteria akceptacji).
*   Zakończony development funkcjonalności przewidzianej do testowania (kod w repozytorium, build dostępny).
*   Przygotowane środowisko testowe i dane testowe.
*   Dostępne i skonfigurowane narzędzia testowe.
*   Ukończone i "zielone" testy niższego poziomu (np. testy jednostkowe przed integracyjnymi).

### 8.2. Kryteria Wyjścia (Zakończenia danego etapu/całości testów)

*   Wykonanie 100% zaplanowanych scenariuszy testowych dla danego etapu/wydania.
*   Osiągnięcie docelowego pokrycia kodu testami (np. >80% dla testów jednostkowych w `Services` i `Controllers`).
*   Brak otwartych błędów o priorytecie Krytycznym (Blocker/Showstopper).
*   Liczba otwartych błędów o priorytecie Wysokim nie przekracza X (np. 0-1, do ustalenia z zespołem).
*   Liczba otwartych błędów o priorytecie Średnim i Niskim mieści się w akceptowalnych granicach (np. <5 średnich, <10 niskich).
*   Wszystkie zidentyfikowane błędy są zaraportowane, przeanalizowane i mają określony status (Naprawiony, Odrzucony, Do zrobienia później).
*   Pozytywne wyniki testów UAT (jeśli dotyczy danego etapu).
*   Przygotowany i zaakceptowany raport z testów.

## 9. Role i Odpowiedzialności w Procesie Testowania

*   **Deweloperzy:**
    *   Tworzenie i utrzymanie testów jednostkowych dla swojego kodu.
    *   Wsparcie w tworzeniu i debugowaniu testów integracyjnych.
    *   Naprawa błędów zgłoszonych przez QA i wynikających z testów automatycznych.
    *   Uczestnictwo w przeglądach kodu pod kątem testowalności.
*   **Inżynier QA / Tester:**
    *   Opracowanie i aktualizacja Planu Testów.
    *   Projektowanie, implementacja i utrzymanie testów automatycznych (integracyjne, API, E2E, komponentów UI).
    *   Wykonywanie testów manualnych i eksploracyjnych.
    *   Konfiguracja i zarządzanie danymi testowymi oraz środowiskami testowymi.
    *   Raportowanie, priorytetyzacja i śledzenie błędów.
    *   Analiza wyników testów i przygotowywanie raportów.
    *   Współpraca z deweloperami w celu zapewnienia jakości.
*   **Product Owner / Właściciel Produktu:**
    *   Definiowanie kryteriów akceptacji dla historyjek użytkownika.
    *   Uczestnictwo w testach UAT i ostateczna akceptacja funkcjonalności.
    *   Priorytetyzacja naprawy błędów z perspektywy biznesowej.
*   **Zespół DevOps (jeśli wyodrębniony):**
    *   Zapewnienie i utrzymanie infrastruktury dla środowisk testowych.
    *   Integracja testów automatycznych z procesem CI/CD.
    *   Monitorowanie stabilności środowisk testowych.

## 10. Procedury Raportowania Błędów

*   Wszystkie zidentyfikowane defekty (błędy) będą raportowane jako "Issues" w systemie GitHub.
*   Każdy raport o błędzie powinien zawierać:
    *   **Tytuł:** Zwięzły opis problemu, np. "Błąd walidacji przy próbie zapisu profilu bez adresu email".
    *   **Kroki do reprodukcji:** Numerowana lista kroków prowadzących do wystąpienia błędu.
    *   **Oczekiwany rezultat:** Co powinno się wydarzyć.
    *   **Rzeczywisty rezultat:** Co faktycznie się wydarzyło.
    *   **Środowisko:** Wersja aplikacji (commit SHA), przeglądarka (jeśli dotyczy UI), system operacyjny, konkretne dane testowe użyte.
    *   **Priorytet:** (np. Krytyczny, Wysoki, Średni, Niski) - określa pilność naprawy.
    *   **Ważność (Severity):** (np. Krytyczna, Poważna, Drobna, Kosmetyczna) - określa wpływ błędu na system.
    *   **Załączniki:** Zrzuty ekranu, logi (z konsoli przeglądarki, serwera), krótkie nagrania wideo.
    *   **Etykiety (Labels):** `bug`, nazwa modułu (np. `profile-management`, `plan-generation`), priorytet, status.
*   **Cykl życia błędu (proponowane statusy jako etykiety GitHub):**
    *   `status:new` - Nowo zgłoszony błąd.
    *   `status:confirmed` - Błąd potwierdzony, oczekuje na przypisanie/naprawę.
    *   `status:in-progress` - Trwa praca nad naprawą błędu.
    *   `status:ready-for-test` - Błąd naprawiony, gotowy do weryfikacji przez QA.
    *   `status:closed` - Błąd zweryfikowany jako naprawiony.
    *   `status:rejected` - Zgłoszenie nie jest błędem lub nie będzie naprawiane.
    *   `status:on-hold` - Naprawa odroczona.
*   Regularne przeglądy zgłoszonych błędów (Bug Triage) przez zespół w celu ustalenia priorytetów i planu napraw.