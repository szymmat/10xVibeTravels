<conversation_summary>
<decisions>
1.  **Platforma:** Aplikacja webowa.
2.  **Zarządzanie Kontem:** Rejestracja przez e-mail/hasło, logowanie. Reset hasła poza MVP.
3.  **Profil Użytkownika:**
    *   Preferencje: Budżet (liczba), Zainteresowania (wybór z: Historia, Sztuka, Przyroda, Życie nocne, Jedzenie, Plaże), Styl podróży (wybór z: Luksusowy, Budżetowy, Przygodowy, All-inclusive, Objazdowa), Intensywność (wybór z: Relaksacyjny, Umiarkowany, Intensywny).
    *   Zachęta do wypełnienia: Jednorazowy komunikat po rejestracji.
4.  **Zarządzanie Notatkami (CRUD):**
    *   Tworzenie/Edycja: Tytuł (max 100 znaków), Treść (tekst, max 2000 znaków). Walidacja długości.
    *   Przeglądanie: Lista w formie kart z podglądem 150 znaków treści. Sortowanie wg daty utworzenia/modyfikacji. Paginacja (10/stronę).
    *   Usuwanie: Z potwierdzeniem (okno dialogowe).
    *   Wyszukiwanie: Poza MVP.
5.  **Generowanie Planów przez AI:**
    *   Inicjacja: Przycisk w widoku notatki.
    *   Dane wejściowe: Notatka (tytuł+treść), preferencje z profilu, Data "od" i "do" (wymagane, walidacja do > od), Budżet (wymagany, walidacja >= 0; priorytet ma budżet podany przy generowaniu).
    *   Proces: Wyświetlenie wskaźnika ładowania. Generowanie 3 różnych wariantów planów (różnicowanie wg stylu, intensywności, zainteresowań, lokalizacji - zależne od danych wejściowych).
    *   Wynik: 3 propozycje planów wyświetlone jako karty do porównania. Każdy plan zawiera: Tytuł (generowany przez AI, edytowalny przed zapisem, max 100 znaków, krótki, opisowy), Treść (lista miejsc, sugerowane trasy dzienne, opisy atrakcji, ogólne sugestie transportu).
    *   Obsługa błędów AI: Jeśli AI nie może wygenerować planów, wyświetlany jest komunikat opakowujący surową odpowiedź AI z prośbą o doprecyzowanie notatki.
6.  **Akceptacja/Odrzucenie Planów:**
    *   Interfejs: Każda karta propozycji ma ikonę edycji tytułu oraz przyciski "Akceptuj" i "Odrzuć".
    *   Akceptacja: Użytkownik może zaakceptować jeden lub więcej planów. Po kliknięciu "Akceptuj", przycisk znika, karta zmienia wygląd (np. kolor). Edycja tytułu możliwa przed akceptacją.
    *   Odrzucenie: Użytkownik może odrzucić pojedyncze propozycje lub opuścić widok porównania bez akceptacji.
    *   Zapis: Każdy zaakceptowany plan jest zapisywany jako osobny byt w systemie.
    *   Nawigacja: Po zakończeniu interakcji z propozycjami (akceptacja/odrzucenie/opuszczenie widoku), użytkownik jest przenoszony do listy zapisanych planów.
7.  **Zarządzanie Zapisanymi Planami (CRUD):**
    *   Przeglądanie: Lista w formie kart z podglądem 150 znaków treści. Sortowanie wg daty utworzenia/modyfikacji. Paginacja (10/stronę).
    *   Edycja: Możliwość edycji treści planu w polu tekstowym o dowolnym formacie (plain text).
    *   Usuwanie: Z potwierdzeniem (okno dialogowe).
8.  **Relacje:** Notatki i plany są osobnymi bytami. Usunięcie notatki nie usuwa wygenerowanych z niej planów. Edycja notatki nie wpływa na istniejące plany. Zmiana profilu nie wpływa na już wygenerowane (ale niezaakceptowane) propozycje.
9.  **Limity:** Brak limitów liczby notatek i planów w MVP.
10. **Empty States:** Zdefiniowane teksty dla pustych list notatek i planów.
11. **Kryteria sukcesu (Monitorowanie w MVP):** Dane o % wypełnionych profili i liczbie generowanych planów dostępne do wglądu w bazie danych (bez dedykowanego interfejsu dla użytkownika/admina w MVP).
12. **Wymagania niefunkcjonalne:** Brak specyficznych wymagań dla MVP poza podstawową walidacją i obsługą błędów.

</decisions>

<matched_recommendations>
1.  Zdefiniowano szczegółowo preferencje użytkownika i sposób ich zbierania (komunikat po rejestracji).
2.  Określono strukturę i zawartość generowanego planu AI oraz sposób jego prezentacji (3 karty).
3.  Ustalono mechanizm oceny planów (akceptacja/odrzucenie) i przepływ użytkownika po akceptacji.
4.  Zdefiniowano podstawowe funkcjonalności CRUD dla notatek i planów, w tym limity znaków i walidację.
5.  Sprecyzowano relację między notatkami a planami.
6.  Określono listę zainteresowań, stylów podróży i intensywności.
7.  Zaprojektowano obsługę błędów generowania AI (opakowany komunikat).
8.  Zdecydowano o sposobie edycji zapisanych planów (wolny format tekstowy).
9.  Określono mechanizm generowania i edycji tytułów planów.
10. Potwierdzono brak wpływu edycji notatek/profilu na istniejące/wygenerowane plany.
11. Zaimplementowano walidację danych wejściowych (daty, budżet, długości tekstów).
12. Zaprojektowano sposób obsługi usuwania (okna dialogowe) i pustych stanów.
13. Ustalono sposób prezentacji list (karty z podglądem) oraz sortowanie i paginację.
14. Zdefiniowano zachowanie przy ponownym generowaniu planów z tej samej notatki.
15. Dodano wskaźnik ładowania podczas generowania AI.
16. Określono sposób odrzucania propozycji AI.
</matched_recommendations>

<prd_planning_summary>
**Produkt:** VibeTravels (MVP) - Aplikacja webowa.

**Problem Użytkownika:** Trudność w planowaniu angażujących wycieczek na podstawie luźnych notatek.

**Cel MVP:** Umożliwienie użytkownikom zapisywania notatek podróżniczych i konwertowania ich za pomocą AI na spersonalizowane plany wycieczek, oparte na preferencjach użytkownika.

**Główne Wymagania Funkcjonalne:**
*   **Zarządzanie Kontem:** Rejestracja (email/hasło), logowanie.
*   **Profil Użytkownika:** Definiowanie preferencji (budżet, zainteresowania, styl, intensywność). Jednorazowa zachęta do wypełnienia.
*   **Zarządzanie Notatkami:** Tworzenie, odczyt, aktualizacja, usuwanie (CRUD) notatek z tytułem (do 100 znaków) i treścią (do 2000 znaków). Wyświetlanie listy notatek (karty z podglądem 150 znaków, sortowanie, paginacja).
*   **Generowanie Planów AI:**
    *   Uruchamiane z poziomu notatki.
    *   Wykorzystuje notatkę, profil, daty i budżet.
    *   Generuje 3 różne propozycje planów (tytuł + treść z trasami/miejscami/opisami/sugestiami transportu).
    *   Wyświetla propozycje w formie kart do porównania.
    *   Obsługuje błędy generowania, prosząc o doprecyzowanie notatki (z sugestią AI).
    *   Wyświetla wskaźnik ładowania.
*   **Interakcja z Propozycjami:** Możliwość edycji tytułu propozycji, akceptacji (zapis jako osobny plan) lub odrzucenia każdej z 3 propozycji.
*   **Zarządzanie Zapisanymi Planami:** Przeglądanie (lista kart z podglądem 150 znaków, sortowanie, paginacja), edycja (wolny format tekstowy), usuwanie (z potwierdzeniem).

**Kluczowe Historie Użytkownika / Ścieżki Korzystania:**
1.  **Rejestracja i Onboarding:** Użytkownik rejestruje się, loguje, widzi jednorazową zachętę do wypełnienia profilu, opcjonalnie wypełnia preferencje.
2.  **Tworzenie i Zarządzanie Notatkami:** Użytkownik tworzy nową notatkę (tytuł, treść), przegląda listę swoich notatek, edytuje istniejącą notatkę, usuwa notatkę.
3.  **Generowanie Planu:** Użytkownik w widoku notatki klika "Generuj plany", podaje daty i budżet, czeka na wyniki AI (wskaźnik ładowania).
4.  **Wybór Planu:** Użytkownik porównuje 3 wygenerowane propozycje (karty), edytuje tytuły (opcjonalnie), akceptuje jedną lub więcej propozycji, odrzuca niechciane, lub opuszcza widok.
5.  **Zarządzanie Planami:** Użytkownik przegląda listę zapisanych planów, otwiera plan do przeglądania, edytuje treść zapisanego planu, usuwa plan.

**Ograniczenia MVP (Co NIE Wchodzi w Zakres):**
*   Współdzielenie planów.
*   Obsługa multimediów.
*   Zaawansowane planowanie logistyki (konkretne loty, rezerwacje).
*   Reset hasła.
*   Wyszukiwanie notatek.
*   Zaawansowany edytor tekstu dla planów (tylko plain text).
*   Aktywne mechanizmy zachęcające do wypełnienia profilu (poza jednorazowym komunikatem).
*   Interfejs do śledzenia kryteriów sukcesu.
*   Szczegółowy feedback na temat odrzuconych propozycji AI.

</prd_planning_summary>
</conversation_summary>