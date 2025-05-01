# Dokument wymagań produktu (PRD) - VibeTravels (MVP)

## 1. Przegląd produktu

VibeTravels (MVP) to aplikacja webowa zaprojektowana, aby pomóc użytkownikom w łatwym planowaniu angażujących wycieczek. Główną funkcją aplikacji jest konwersja luźnych notatek podróżniczych użytkowników na spersonalizowane, szczegółowe plany wycieczek przy użyciu sztucznej inteligencji (AI). Plany te są dostosowywane w oparciu o zdefiniowane przez użytkownika preferencje, takie jak budżet, zainteresowania, styl podróży i pożądana intensywność. MVP skupia się na podstawowych funkcjonalnościach zarządzania notatkami, profilami użytkowników, generowaniu planów przez AI oraz zarządzaniu zapisanymi planami.

## 2. Problem użytkownika

Planowanie interesujących i angażujących wycieczek, które odpowiadają indywidualnym preferencjom, bywa czasochłonne i trudne. Użytkownicy często zaczynają od luźnych pomysłów i notatek, ale przekształcenie ich w konkretny, wykonalny plan podróży stanowi wyzwanie. Brakuje narzędzia, które mogłoby w inteligentny sposób wykorzystać te wstępne pomysły i preferencje do stworzenia gotowych propozycji planów.

## 3. Wymagania funkcjonalne

Aplikacja VibeTravels (MVP) będzie posiadała następujące funkcjonalności:

3.1.  Zarządzanie Kontem:
    *   Rejestracja użytkownika za pomocą adresu e-mail i hasła.
    *   Logowanie użytkownika.

3.2.  Profil Użytkownika:
    *   Możliwość zdefiniowania preferencji podróżniczych:
        *   Budżet (wartość liczbowa).
        *   Zainteresowania (wybór z listy: Historia, Sztuka, Przyroda, Życie nocne, Jedzenie, Plaże).
        *   Styl podróży (wybór z listy: Luksusowy, Budżetowy, Przygodowy).
        *   Intensywność (wybór z listy: Relaksacyjny, Umiarkowany, Intensywny).
    *   Wyświetlenie jednorazowego komunikatu zachęcającego do wypełnienia profilu po pierwszej rejestracji/logowaniu.

3.3.  Zarządzanie Notatkami (CRUD):
    *   Tworzenie nowej notatki z tytułem (max 100 znaków) i treścią (max 2000 znaków). Walidacja długości pól.
    *   Przeglądanie listy notatek w formie kart, każda z podglądem 150 znaków treści.
    *   Sortowanie listy notatek wg daty utworzenia lub modyfikacji (malejąco).
    *   Paginacja listy notatek (10 notatek na stronę).
    *   Edycja istniejącej notatki (tytuł i treść).
    *   Usuwanie notatki z potwierdzeniem (okno dialogowe).
    *   Wyświetlanie komunikatu "empty state", gdy brak notatek.

3.4.  Generowanie Planów przez AI:
    *   Inicjacja procesu generowania z poziomu widoku konkretnej notatki za pomocą dedykowanego przycisku.
    *   Wymagane dane wejściowe: Data "od" i "do" podróży (walidacja: data "do" musi być późniejsza niż data "od"), Budżet (walidacja: wartość >= 0). Budżet podany przy generowaniu ma priorytet nad budżetem z profilu.
    *   Wykorzystanie treści notatki (tytuł + treść) oraz preferencji z profilu użytkownika jako danych wejściowych dla AI.
    *   Wyświetlanie wskaźnika ładowania podczas procesu generowania.
    *   Generowanie 3 różnych wariantów planów podróży przez AI, różnicowanych pod kątem stylu, intensywności, zainteresowań lub lokalizacji (w zależności od danych wejściowych).
    *   Prezentacja 3 wygenerowanych propozycji planów w formie kart, umożliwiających porównanie. Każdy plan zawiera:
        *   Tytuł (generowany przez AI, edytowalny przed akceptacją, max 100 znaków).
        *   Treść (zawierająca sugerowane miejsca, trasy dzienne, opisy atrakcji, ogólne sugestie dotyczące transportu).
    *   Obsługa błędów: W przypadku niepowodzenia generowania przez AI, wyświetlenie komunikatu informującego o problemie i sugerującego doprecyzowanie notatki (komunikat może zawierać opakowaną surową odpowiedź AI).

3.5.  Interakcja z Propozycjami Planów:
    *   Możliwość edycji tytułu każdej z 3 propozycji planów przed akceptacją.
    *   Możliwość akceptacji jednej lub więcej propozycji planów za pomocą przycisku "Akceptuj". Zaakceptowany plan jest zapisywany jako osobny byt w systemie. Przycisk "Akceptuj" znika po kliknięciu, a karta wizualnie oznacza akceptację.
    *   Możliwość odrzucenia pojedynczych propozycji za pomocą przycisku "Odrzuć" lub przez opuszczenie widoku porównania bez akceptacji.
    *   Przeniesienie użytkownika do listy zapisanych planów po zakończeniu interakcji z propozycjami (akceptacja/odrzucenie/opuszczenie widoku).

3.6.  Zarządzanie Zapisanymi Planami (CRUD):
    *   Przeglądanie listy zapisanych planów w formie kart, każda z podglądem 150 znaków treści.
    *   Sortowanie listy zapisanych planów wg daty utworzenia lub modyfikacji (malejąco).
    *   Paginacja listy zapisanych planów (10 planów na stronę).
    *   Edycja treści zapisanego planu w prostym polu tekstowym (plain text).
    *   Usuwanie zapisanego planu z potwierdzeniem (okno dialogowe).
    *   Wyświetlanie komunikatu "empty state", gdy brak zapisanych planów.

3.7.  Relacje Danych:
    *   Notatki i plany są osobnymi bytami.
    *   Usunięcie notatki nie powoduje usunięcia planów wygenerowanych na jej podstawie.
    *   Edycja notatki nie wpływa na już istniejące plany (wygenerowane lub zapisane).
    *   Zmiana preferencji w profilu nie wpływa na już wygenerowane (ale jeszcze niezaakceptowane) propozycje planów.

3.8.  Limity:
    *   Brak limitów liczby notatek i zapisanych planów dla użytkownika w ramach MVP.

## 4. Granice produktu

Następujące funkcjonalności i cechy NIE wchodzą w zakres MVP:

*   Resetowanie hasła użytkownika.
*   Współdzielenie notatek lub planów z innymi użytkownikami.
*   Obsługa i dodawanie multimediów (np. zdjęć, filmów) do notatek lub planów.
*   Zaawansowane planowanie logistyczne (np. rezerwacja lotów, hoteli, sprawdzanie dostępności w czasie rzeczywistym).
*   Wyszukiwanie notatek lub planów po treści lub słowach kluczowych.
*   Zaawansowany edytor tekstu (rich text editor) dla treści notatek lub planów (tylko plain text).
*   Aktywne, powtarzalne mechanizmy zachęcające do wypełnienia profilu użytkownika (poza jednorazowym komunikatem po rejestracji).
*   Dedykowany interfejs dla administratora lub użytkownika do przeglądania metryk sukcesu.
*   Mechanizm zbierania szczegółowego feedbacku od użytkownika na temat odrzuconych propozycji AI.
*   Powiadomienia w aplikacji lub e-mail.

## 5. Historyjki użytkowników

### 5.1. Zarządzanie Kontem i Profilem

ID: US-001
Tytuł: Rejestracja nowego użytkownika
Opis: Jako nowy użytkownik, chcę móc założyć konto w aplikacji VibeTravels, podając mój adres e-mail i hasło, abym mógł zacząć zapisywać notatki i generować plany podróży.
Kryteria akceptacji:
*   Mogę przejść do formularza rejestracji.
*   Formularz wymaga podania adresu e-mail i hasła (z potwierdzeniem).
*   System waliduje poprawność formatu adresu e-mail.
*   System waliduje siłę hasła (np. minimalna długość).
*   System sprawdza, czy e-mail nie jest już zarejestrowany.
*   Po poprawnej walidacji i przesłaniu formularza, moje konto jest tworzone.
*   Jestem automatycznie zalogowany lub przekierowany na stronę logowania.
*   W przypadku błędu (np. e-mail zajęty, nieprawidłowe dane) wyświetlany jest czytelny komunikat.

ID: US-002
Tytuł: Logowanie do aplikacji
Opis: Jako zarejestrowany użytkownik, chcę móc zalogować się do aplikacji VibeTravels za pomocą mojego adresu e-mail i hasła, aby uzyskać dostęp do moich notatek i planów.
Kryteria akceptacji:
*   Mogę przejść do formularza logowania.
*   Formularz wymaga podania adresu e-mail i hasła.
*   System weryfikuje poprawność podanych danych uwierzytelniających.
*   Po pomyślnym zalogowaniu jestem przekierowany do głównego widoku aplikacji (np. lista notatek).
*   W przypadku podania nieprawidłowych danych (zły e-mail lub hasło) wyświetlany jest czytelny komunikat błędu.
*   Sesja użytkownika jest utrzymywana po zalogowaniu.

ID: US-003
Tytuł: Wypełnienie profilu użytkownika po raz pierwszy
Opis: Jako nowy, zalogowany użytkownik, chcę zobaczyć jednorazowy komunikat zachęcający mnie do wypełnienia preferencji podróżniczych w moim profilu, abym mógł otrzymywać bardziej spersonalizowane plany podróży.
Kryteria akceptacji:
*   Po pierwszym zalogowaniu po rejestracji widzę wyraźny komunikat/prompt sugerujący wypełnienie profilu.
*   Komunikat zawiera przycisk/link przenoszący mnie do strony/sekcji edycji profilu.
*   Mogę zignorować/zamknąć ten komunikat.
*   Komunikat nie pojawia się przy kolejnych logowaniach.

ID: US-004
Tytuł: Edycja preferencji w profilu użytkownika
Opis: Jako zalogowany użytkownik, chcę móc zdefiniować lub zaktualizować moje preferencje podróżnicze (budżet, zainteresowania, styl podróży, intensywność) na stronie mojego profilu, aby AI mogła lepiej dopasować generowane plany.
Kryteria akceptacji:
*   Mogę przejść do strony/sekcji edycji mojego profilu.
*   Widzę pola do wprowadzenia/wyboru preferencji: Budżet (pole liczbowe), Zainteresowania (lista wielokrotnego wyboru), Styl podróży (lista jednokrotnego wyboru), Intensywność (lista jednokrotnego wyboru).
*   Mogę zapisać wprowadzone zmiany.
*   Zapisane preferencje są widoczne przy kolejnym wejściu na stronę profilu.
*   Preferencje są wykorzystywane podczas generowania planów AI (chyba że nadpisane podczas generowania, np. budżet).

### 5.2. Zarządzanie Notatkami

ID: US-101
Tytuł: Tworzenie nowej notatki podróżniczej
Opis: Jako zalogowany użytkownik, chcę móc stworzyć nową notatkę podróżniczą, podając jej tytuł i treść, aby zapisać moje pomysły na przyszłe wycieczki.
Kryteria akceptacji:
*   Mogę zainicjować tworzenie nowej notatki (np. przez przycisk "Dodaj notatkę").
*   Widzę formularz z polami na Tytuł i Treść.
*   Pole Tytuł ma ograniczenie do 100 znaków; po przekroczeniu limitu nie mogę wpisać więcej znaków lub pojawia się walidacja.
*   Pole Treść ma ograniczenie do 2000 znaków; po przekroczeniu limitu nie mogę wpisać więcej znaków lub pojawia się walidacja.
*   Mogę zapisać notatkę.
*   Po zapisaniu notatka pojawia się na liście moich notatek.
*   Jeśli spróbuję zapisać pustą notatkę (bez tytułu lub treści), pojawi się komunikat błędu.

ID: US-102
Tytuł: Przeglądanie listy notatek
Opis: Jako zalogowany użytkownik, chcę móc przeglądać listę moich notatek podróżniczych, aby szybko znaleźć te, które mnie interesują.
Kryteria akceptacji:
*   Widzę listę moich notatek po zalogowaniu lub przejściu do odpowiedniej sekcji.
*   Notatki są wyświetlane jako karty.
*   Każda karta pokazuje tytuł notatki i podgląd jej treści (pierwsze 150 znaków).
*   Lista jest domyślnie sortowana według daty utworzenia lub modyfikacji (od najnowszej).
*   Jeśli notatek jest więcej niż 10, widzę mechanizm paginacji (np. przyciski "Następna"/"Poprzednia" strona lub numery stron).
*   Paginacja pozwala mi przechodzić między stronami listy.

ID: US-103
Tytuł: Widok pustej listy notatek
Opis: Jako zalogowany użytkownik, który nie ma jeszcze żadnych notatek, chcę zobaczyć czytelny komunikat informujący o braku notatek i zachęcający do ich stworzenia.
Kryteria akceptacji:
*   Gdy przechodzę do listy notatek i nie mam żadnych zapisanych, widzę specjalny komunikat (empty state).
*   Komunikat jasno informuje, że lista jest pusta.
*   Komunikat zawiera przycisk/link umożliwiający szybkie przejście do tworzenia nowej notatki.

ID: US-104
Tytuł: Edycja istniejącej notatki
Opis: Jako zalogowany użytkownik, chcę móc edytować tytuł i treść istniejącej notatki, aby zaktualizować moje pomysły lub poprawić błędy.
Kryteria akceptacji:
*   Na liście notatek lub w widoku szczegółów notatki mam możliwość zainicjowania edycji (np. przycisk "Edytuj").
*   Po kliknięciu "Edytuj" widzę formularz z wypełnionymi aktualnym tytułem i treścią notatki.
*   Mogę modyfikować tytuł (z zachowaniem limitu 100 znaków) i treść (z zachowaniem limitu 2000 znaków).
*   Mogę zapisać zmiany.
*   Po zapisaniu zmian, zaktualizowana notatka jest widoczna na liście i w widoku szczegółów.
*   Mogę anulować edycję bez zapisywania zmian.

ID: US-105
Tytuł: Usuwanie notatki
Opis: Jako zalogowany użytkownik, chcę móc usunąć notatkę, której już nie potrzebuję.
Kryteria akceptacji:
*   Na liście notatek lub w widoku szczegółów notatki mam możliwość zainicjowania usunięcia (np. przycisk "Usuń").
*   Po kliknięciu "Usuń" pojawia się okno dialogowe z prośbą o potwierdzenie usunięcia.
*   Jeśli potwierdzę, notatka jest trwale usuwana z systemu.
*   Notatka znika z mojej listy notatek.
*   Jeśli anuluję operację w oknie dialogowym, notatka nie jest usuwana.
*   Usunięcie notatki nie usuwa planów, które mogły zostać z niej wygenerowane.

### 5.3. Generowanie Planów AI

ID: US-201
Tytuł: Inicjowanie generowania planu AI z notatki
Opis: Jako zalogowany użytkownik, przeglądając szczegóły konkretnej notatki, chcę móc uruchomić proces generowania planów podróży przez AI na jej podstawie.
Kryteria akceptacji:
*   W widoku szczegółów notatki (lub na karcie notatki na liście) widoczny jest przycisk "Generuj plany" (lub podobny).
*   Kliknięcie przycisku inicjuje proces generowania.
*   System prosi mnie o podanie dodatkowych danych: Daty "od" i "do" podróży oraz opcjonalnie Budżetu.

ID: US-202
Tytuł: Podawanie szczegółów do generowania planu
Opis: Jako zalogowany użytkownik, po zainicjowaniu generowania planu, chcę móc podać wymagane daty podróży ("od" i "do") oraz opcjonalnie budżet, aby AI mogła stworzyć dopasowane propozycje.
Kryteria akceptacji:
*   Widzę formularz/modal do wprowadzenia daty początkowej i końcowej podróży (np. date pickery).
*   Widzę pole do wprowadzenia budżetu (liczbowe).
*   System waliduje daty: data "do" musi być późniejsza niż data "od".
*   System waliduje budżet: musi być wartością liczbową nieujemną (>= 0).
*   W przypadku błędnej walidacji, widzę komunikat i nie mogę kontynuować.
*   Jeśli nie podam budżetu, system użyje budżetu z mojego profilu (jeśli jest zdefiniowany). Jeśli podam budżet tutaj, ma on priorytet nad tym z profilu.
*   Po poprawnym wprowadzeniu danych mogę zatwierdzić i rozpocząć generowanie.

ID: US-203
Tytuł: Obserwowanie procesu generowania planu
Opis: Jako zalogowany użytkownik, po zatwierdzeniu danych do generowania planu, chcę widzieć wskaźnik postępu (ładowania), abym wiedział, że system pracuje nad moim zapytaniem.
Kryteria akceptacji:
*   Po kliknięciu przycisku rozpoczynającego generowanie (po podaniu dat/budżetu), widzę wyraźny wskaźnik ładowania (np. spinner, pasek postępu).
*   Wskaźnik jest widoczny do momentu otrzymania wyników (propozycji planów) lub błędu od AI.

ID: US-204
Tytuł: Otrzymanie i porównanie propozycji planów AI
Opis: Jako zalogowany użytkownik, po zakończeniu generowania przez AI, chcę zobaczyć 3 różne propozycje planów podróży przedstawione w formie kart, abym mógł je porównać.
Kryteria akceptacji:
*   Po pomyślnym wygenerowaniu przez AI, wskaźnik ładowania znika.
*   Widzę 3 odrębne karty, każda reprezentująca jedną propozycję planu.
*   Każda karta zawiera:
    *   Tytuł planu (wygenerowany przez AI, np. "Przygoda w Bieszczadach", "Kulturalny weekend w Krakowie").
    *   Treść planu (podsumowanie, sugerowane miejsca, trasy, opisy, wskazówki transportowe).
*   Karty są przedstawione w sposób umożliwiający łatwe porównanie (np. obok siebie lub jedna pod drugą).

ID: US-205
Tytuł: Obsługa błędu generowania planu przez AI
Opis: Jako zalogowany użytkownik, w przypadku gdy AI nie jest w stanie wygenerować planów na podstawie mojej notatki i preferencji, chcę otrzymać czytelny komunikat o błędzie wraz z sugestią, jak mogę poprawić dane wejściowe.
Kryteria akceptacji:
*   Jeśli proces generowania AI zakończy się niepowodzeniem, wskaźnik ładowania znika.
*   Zamiast propozycji planów widzę komunikat informujący o problemie z generowaniem.
*   Komunikat zawiera sugestię, aby spróbować ponownie, np. poprzez doprecyzowanie treści notatki (może zawierać ogólną wskazówkę zwróconą przez AI, ale w sposób bezpieczny i zrozumiały dla użytkownika).
*   Komunikat pozwala mi wrócić do widoku notatki.

### 5.4. Interakcja z Propozycjami Planów

ID: US-301
Tytuł: Edycja tytułu propozycji planu AI
Opis: Jako zalogowany użytkownik, przeglądając wygenerowane propozycje planów, chcę móc edytować tytuł każdej z nich przed jej akceptacją, aby nadać mu bardziej osobisty charakter.
Kryteria akceptacji:
*   Na każdej karcie propozycji planu widzę jej tytuł wygenerowany przez AI.
*   Mam możliwość edycji tego tytułu (np. ikona edycji obok tytułu).
*   Po kliknięciu w ikonę edycji mogę zmienić tekst tytułu.
*   Zmodyfikowany tytuł ma ograniczenie do 100 znaków.
*   Zmiana tytułu jest widoczna na karcie propozycji.
*   Edycja tytułu jest możliwa tylko przed akceptacją planu.

ID: US-302
Tytuł: Akceptacja propozycji planu AI
Opis: Jako zalogowany użytkownik, po przejrzeniu propozycji planów, chcę móc zaakceptować jedną lub więcej z nich, które mi odpowiadają, aby zapisać je na moim koncie.
Kryteria akceptacji:
*   Każda karta propozycji planu posiada przycisk "Akceptuj".
*   Kliknięcie przycisku "Akceptuj" powoduje zapisanie danego planu (z aktualnym tytułem i treścią) jako osobnego planu na moim koncie.
*   Po kliknięciu "Akceptuj", przycisk ten znika lub staje się nieaktywny dla danej propozycji.
*   Karta zaakceptowanego planu wizualnie zmienia wygląd (np. zmiana koloru tła, dodanie znacznika "Zaakceptowano").
*   Mogę zaakceptować więcej niż jedną propozycję.

ID: US-303
Tytuł: Odrzucenie propozycji planu AI
Opis: Jako zalogowany użytkownik, chcę mieć możliwość odrzucenia propozycji planu, która mi nie odpowiada, lub opuszczenia widoku porównania bez akceptowania żadnej propozycji.
Kryteria akceptacji:
*   Każda karta propozycji może mieć przycisk "Odrzuć" (opcjonalnie, alternatywnie użytkownik po prostu nie klika "Akceptuj").
*   Kliknięcie "Odrzuć" (jeśli istnieje) powoduje wizualne oznaczenie karty jako odrzuconej lub jej ukrycie.
*   Mogę opuścić widok porównania propozycji (np. przez przycisk "Wróć" lub nawigację w aplikacji) bez konieczności akceptowania czy odrzucania każdej propozycji.
*   Odrzucone propozycje nie są zapisywane na moim koncie.

ID: US-304
Tytuł: Przejście do listy zapisanych planów po interakcji
Opis: Jako zalogowany użytkownik, po zakończeniu interakcji z propozycjami AI (akceptacji, odrzuceniu lub opuszczeniu widoku), chcę zostać automatycznie przeniesiony do listy moich zapisanych planów.
Kryteria akceptacji:
*   Po opuszczeniu widoku porównania propozycji (np. przez kliknięcie "Zakończ", "Gotowe", lub automatycznie po akceptacji/odrzuceniu ostatniej), jestem przekierowywany do widoku listy moich zapisanych planów.
*   Jeśli zaakceptowałem jakieś plany, widzę je teraz na tej liście.

### 5.5. Zarządzanie Zapisanymi Planami

ID: US-401
Tytuł: Przeglądanie listy zapisanych planów
Opis: Jako zalogowany użytkownik, chcę móc przeglądać listę moich zapisanych planów podróży.
Kryteria akceptacji:
*   Mogę przejść do sekcji "Moje Plany" (lub podobnej).
*   Widzę listę moich zapisanych (zaakceptowanych wcześniej) planów.
*   Plany są wyświetlane jako karty.
*   Każda karta pokazuje tytuł planu i podgląd jego treści (pierwsze 150 znaków).
*   Lista jest domyślnie sortowana według daty utworzenia (daty akceptacji propozycji) lub modyfikacji (od najnowszej).
*   Jeśli planów jest więcej niż 10, widzę mechanizm paginacji.
*   Paginacja pozwala mi przechodzić między stronami listy.

ID: US-402
Tytuł: Widok pustej listy zapisanych planów
Opis: Jako zalogowany użytkownik, który nie ma jeszcze żadnych zapisanych planów, chcę zobaczyć czytelny komunikat informujący o braku planów.
Kryteria akceptacji:
*   Gdy przechodzę do listy zapisanych planów i nie mam żadnych, widzę specjalny komunikat (empty state).
*   Komunikat jasno informuje, że lista jest pusta i sugeruje, jak stworzyć pierwszy plan (np. poprzez wygenerowanie go z notatki).

ID: US-403
Tytuł: Przeglądanie szczegółów zapisanego planu
Opis: Jako zalogowany użytkownik, chcę móc otworzyć zapisany plan, aby zobaczyć jego pełną treść.
Kryteria akceptacji:
*   Mogę kliknąć na kartę planu na liście, aby przejść do widoku jego szczegółów.
*   W widoku szczegółów widzę pełny tytuł i całą treść zapisanego planu.

ID: US-404
Tytuł: Edycja treści zapisanego planu
Opis: Jako zalogowany użytkownik, chcę móc edytować treść zapisanego planu podróży, aby dostosować go do moich potrzeb lub dodać własne informacje.
Kryteria akceptacji:
*   W widoku szczegółów zapisanego planu mam możliwość zainicjowania edycji treści (np. przycisk "Edytuj treść").
*   Po kliknięciu "Edytuj treść" pole z treścią planu staje się edytowalne (prosty edytor plain text).
*   Mogę modyfikować tekst treści planu.
*   Mogę zapisać wprowadzone zmiany.
*   Zaktualizowana treść jest widoczna w widoku szczegółów planu i podglądzie na liście.
*   Mogę anulować edycję bez zapisywania zmian.
*   Edycja dotyczy tylko treści, nie tytułu (tytuł był edytowalny przed akceptacją).

ID: US-405
Tytuł: Usuwanie zapisanego planu
Opis: Jako zalogowany użytkownik, chcę móc usunąć zapisany plan podróży, którego już nie potrzebuję.
Kryteria akceptacji:
*   Na liście planów lub w widoku szczegółów planu mam możliwość zainicjowania usunięcia (np. przycisk "Usuń").
*   Po kliknięciu "Usuń" pojawia się okno dialogowe z prośbą o potwierdzenie usunięcia.
*   Jeśli potwierdzę, plan jest trwale usuwany z systemu.
*   Plan znika z mojej listy zapisanych planów.
*   Jeśli anuluję operację w oknie dialogowym, plan nie jest usuwany.

## 6. Metryki sukcesu

Sukces MVP VibeTravels będzie mierzony na podstawie następujących kryteriów:

6.1.  Adopcja Profilu Użytkownika:
    *   Cel: Minimum 90% zarejestrowanych użytkowników posiada wypełnione przynajmniej jedno pole preferencji w swoim profilu (poza domyślnymi/pustymi wartościami).
    *   Pomiar: Odsetek kont użytkowników z co najmniej jedną zdefiniowaną preferencją (Budżet, Zainteresowania, Styl, Intensywność).

6.2.  Zaangażowanie w Generowanie Planów:
    *   Cel: Minimum 75% aktywnych użytkowników (np. logujących się przynajmniej raz w miesiącu) generuje co najmniej 3 plany podróży (zaakceptowane) w ciągu roku od rejestracji.
    *   Pomiar: Śledzenie liczby zapisanych (zaakceptowanych) planów powiązanych z kontami użytkowników w określonym przedziale czasowym. 