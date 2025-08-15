# Product Requirements Document (PRD) – VibeTravels (MVP)

## 1. Product Overview

VibeTravels (MVP) is a web application designed to help users easily plan engaging trips. Its core feature is converting users’ loose travel notes into personalized, detailed itineraries using artificial intelligence (AI). These plans are tailored based on user-defined preferences such as budget, interests, travel style, and desired intensity. The MVP focuses on essential features like note management, user profiles, AI-generated plans, and saved plan management.

## 2. User Problem

Planning interesting and engaging trips that match individual preferences can be time-consuming and difficult. Users often start with vague ideas and notes, but turning them into concrete, actionable travel plans is challenging. There’s a lack of tools that can intelligently transform these initial ideas and preferences into ready-made itinerary suggestions.

## 3. Functional Requirements

The VibeTravels (MVP) application will include the following functionalities:

### 3.1. Account Management

- User registration via email and password.
- User login.

### 3.2. User Profile

- Ability to define travel preferences:
  - Budget (numeric value).
  - Interests (select from: History, Art, Nature, Nightlife, Food, Beaches).
  - Travel style (select from: Luxury, Budget, Adventure).
  - Intensity (select from: Relaxed, Moderate, Intense).
- One-time prompt encouraging profile completion after first registration/login.

### 3.3. Note Management (CRUD)

- Create a new note with title (max 100 characters) and content (max 2000 characters). Field length validation.
- View list of notes as cards, each showing a 150-character preview.
- Sort notes by creation or modification date (descending).
- Pagination (10 notes per page).
- Edit existing notes (title and content).
- Delete notes with confirmation dialog.
- Display “empty state” message when no notes are available.

### 3.4. AI Plan Generation

- Initiate generation from a specific note view via a dedicated button.
- Required inputs: travel start and end dates (validation: end date must be after start date), budget (validation: value ≥ 0). Budget entered during generation overrides profile budget.
- Use note content (title + body) and user profile preferences as AI input.
- Show loading indicator during generation.
- Generate 3 different travel plan variants, differentiated by style, intensity, interests, or location (based on input).
- Present 3 generated plans as cards for comparison. Each plan includes:
  - Title (AI-generated, editable before acceptance, max 100 characters).
  - Content (suggested places, daily routes, attraction descriptions, general transport suggestions).
- Error handling: If AI generation fails, display an informative message suggesting note refinement (may include wrapped raw AI response).

### 3.5. Interaction with Plan Proposals

- Edit title of each of the 3 proposed plans before acceptance.
- Accept one or more plans via “Accept” button. Accepted plans are saved as separate entities. Button disappears after click, and card visually indicates acceptance.
- Reject individual proposals via “Reject” button or by leaving the comparison view without accepting.
- Redirect user to saved plans list after interaction (accept/reject/exit view).

### 3.6. Saved Plan Management (CRUD)

- View saved plans as cards, each showing a 150-character preview.
- Sort saved plans by creation or modification date (descending).
- Pagination (10 plans per page).
- Edit saved plan content in a plain text field.
- Delete saved plan with confirmation dialog.
- Display “empty state” message when no saved plans are available.

### 3.7. Data Relationships

- Notes and plans are separate entities.
- Deleting a note does not delete plans generated from it.
- Editing a note does not affect existing plans (generated or saved).
- Changing profile preferences does not affect already generated (but unaccepted) plan proposals.

### 3.8. Limits

- No limit on the number of notes or saved plans per user within the MVP.

## 4. Product Boundaries

The following features are NOT included in the MVP scope:

- Password reset functionality.
- Sharing notes or plans with other users.
- Multimedia support (e.g., photos, videos) in notes or plans.
- Advanced logistical planning (e.g., booking flights, hotels, real-time availability).
- Searching notes or plans by content or keywords.
- Rich text editor for notes or plans (plain text only).
- Active, recurring prompts to complete user profile (beyond one-time prompt).
- Admin or user interface for viewing success metrics.
- Mechanism for collecting detailed user feedback on rejected AI proposals.
- In-app or email notifications.

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
### 5.3. AI Plan Generation

**ID:** US-201  
**Title:** Initiating AI Plan Generation from a Note  
**Description:** As a logged-in user, while viewing the details of a specific note, I want to be able to start the process of generating travel plans using AI based on that note.  
**Acceptance Criteria:**
- A "Generate Plans" button (or similar) is visible in the note detail view (or on the note card in the list).
- Clicking the button initiates the generation process.
- The system prompts me to enter additional data: travel start and end dates, and optionally a budget.

**ID:** US-202  
**Title:** Providing Details for Plan Generation  
**Description:** As a logged-in user, after initiating plan generation, I want to enter the required travel dates ("from" and "to") and optionally a budget so that AI can create tailored suggestions.  
**Acceptance Criteria:**
- I see a form/modal to enter the start and end dates (e.g., date pickers).
- I see a numeric field to enter the budget.
- The system validates the dates: the "to" date must be later than the "from" date.
- The system validates the budget: it must be a non-negative number (≥ 0).
- If validation fails, I see an error message and cannot proceed.
- If no budget is provided, the system uses the budget from my profile (if defined). If a budget is entered here, it overrides the profile budget.
- After entering valid data, I can confirm and start the generation process.

**ID:** US-203  
**Title:** Viewing the Plan Generation Process  
**Description:** As a logged-in user, after confirming the data for plan generation, I want to see a progress indicator (loading) so I know the system is working on my request.  
**Acceptance Criteria:**
- After clicking the button to start generation (after entering dates/budget), I see a clear loading indicator (e.g., spinner, progress bar).
- The indicator remains visible until results (plan suggestions) or an error from AI is received.

**ID:** US-204  
**Title:** Receiving and Comparing AI Plan Suggestions  
**Description:** As a logged-in user, after AI finishes generating, I want to see 3 different travel plan suggestions presented as cards so I can compare them.  
**Acceptance Criteria:**
- After successful AI generation, the loading indicator disappears.
- I see 3 distinct cards, each representing a plan suggestion.
- Each card includes:
  - Plan title (generated by AI, e.g., "Adventure in the Bieszczady Mountains", "Cultural Weekend in Kraków").
  - Plan content (summary, suggested places, routes, descriptions, transport tips).
- Cards are displayed in a way that allows easy comparison (e.g., side-by-side or stacked).

**ID:** US-205  
**Title:** Handling AI Plan Generation Errors  
**Description:** As a logged-in user, if AI fails to generate plans based on my note and preferences, I want to receive a clear error message with suggestions on how to improve the input.  
**Acceptance Criteria:**
- If AI generation fails, the loading indicator disappears.
- Instead of plan suggestions, I see a message indicating a problem with generation.
- The message includes a suggestion to try again, e.g., by refining the note content (may include a general tip returned by AI, presented safely and clearly).
- The message allows me to return to the note view.

### 5.4. Interaction with Plan Suggestions

**ID:** US-301  
**Title:** Editing AI Plan Suggestion Title  
**Description:** As a logged-in user, while reviewing generated plan suggestions, I want to edit the title of each before accepting it to give it a more personal touch.  
**Acceptance Criteria:**
- Each plan suggestion card displays its AI-generated title.
- I can edit the title (e.g., edit icon next to the title).
- Clicking the edit icon allows me to change the title text.
- Modified title is limited to 100 characters.
- The updated title is visible on the suggestion card.
- Title editing is only possible before accepting the plan.

**ID:** US-302  
**Title:** Accepting an AI Plan Suggestion  
**Description:** As a logged-in user, after reviewing the plan suggestions, I want to accept one or more that suit me to save them to my account.  
**Acceptance Criteria:**
- Each plan suggestion card has an "Accept" button.
- Clicking "Accept" saves the plan (with current title and content) as a separate plan on my account.
- After clicking "Accept", the button disappears or becomes inactive for that suggestion.
- The accepted plan card visually changes (e.g., background color change, "Accepted" tag).
- I can accept more than one suggestion.

**ID:** US-303  
**Title:** Rejecting an AI Plan Suggestion  
**Description:** As a logged-in user, I want to reject a plan suggestion that doesn’t suit me or exit the comparison view without accepting any suggestion.  
**Acceptance Criteria:**
- Each suggestion card may have a "Reject" button (optional; alternatively, user simply doesn’t click "Accept").
- Clicking "Reject" (if present) visually marks the card as rejected or hides it.
- I can exit the comparison view (e.g., via "Back" button or app navigation) without accepting or rejecting each suggestion.
- Rejected suggestions are not saved to my account.

**ID:** US-304  
**Title:** Redirecting to Saved Plans List After Interaction  
**Description:** As a logged-in user, after finishing interaction with AI suggestions (accepting, rejecting, or exiting the view), I want to be automatically redirected to my saved plans list.  
**Acceptance Criteria:**
- After leaving the comparison view (e.g., by clicking "Finish", "Done", or automatically after accepting/rejecting the last suggestion), I am redirected to the saved plans list view.
- If I accepted any plans, I now see them in the list.

### 5.5. Saved Plan Management

**ID:** US-401  
**Title:** Viewing Saved Plans List  
**Description:** As a logged-in user, I want to view my list of saved travel plans.  
**Acceptance Criteria:**
- I can access the "My Plans" section (or similar).
- I see a list of my saved (previously accepted) plans.
- Plans are displayed as cards.
- Each card shows the plan title and a preview of its content (first 150 characters).
- The list is sorted by creation date (acceptance date) or modification date (newest first) by default.
- If there are more than 10 plans, I see pagination controls.
- Pagination allows me to navigate between pages of the list.

**ID:** US-402  
**Title:** Empty Saved Plans List View  
**Description:** As a logged-in user with no saved plans yet, I want to see a clear message indicating the absence of plans.  
**Acceptance Criteria:**
- When I access the saved plans list and have none, I see a special message (empty state).
- The message clearly indicates the list is empty and suggests how to create the first plan (e.g., by generating it from a note).

**ID:** US-403  
**Title:** Viewing Saved Plan Details  
**Description:** As a logged-in user, I want to open a saved plan to view its full content.  
**Acceptance Criteria:**
- I can click on a plan card in the list to access its detail view.
- In the detail view, I see the full title and complete content of the saved plan.

**ID:** US-404  
**Title:** Editing Saved Plan Content  
**Description:** As a logged-in user, I want to edit the content of a saved travel plan to customize it or add my own information.  
**Acceptance Criteria:**
- In the saved plan detail view, I can initiate content editing (e.g., "Edit Content" button).
- After clicking "Edit Content", the plan content field becomes editable (simple plain text editor).
- I can modify the plan content text.
- I can save the changes.
- The updated content is visible in the detail view and preview on the list.
- I can cancel the edit without saving changes.
- Editing applies only to content, not the title (title was editable before acceptance).

**ID:** US-405  
**Title:** Deleting a Saved Plan  
**Description:** As a logged-in user, I want to delete a saved travel plan I no longer need.  
**Acceptance Criteria:**
- On the plans list or in the plan detail view, I can initiate deletion (e.g., "Delete" button).
- After clicking "Delete", a confirmation dialog appears.
- If I confirm, the plan is permanently removed from the system.
- The plan disappears from my saved plans list.
- If I cancel the operation in the dialog, the plan is not deleted.

## 6. Success Metrics

The success of the VibeTravels MVP will be measured based on the following criteria:

### 6.1. User Profile Adoption
- **Goal:** At least 90% of registered users have filled in at least one preference field in their profile (excluding default/empty values).
- **Measurement:** Percentage of user accounts with at least one defined preference (Budget, Interests, Style, Intensity).

### 6.2. Engagement with Plan Generation
- **Goal:** At least 75% of active users (e.g., those logging in at least once per month) generate at least 3 travel plans (accepted) within one year of registration.
- **Measurement:** Tracking the number of saved (accepted) plans linked to user accounts within a defined time period.