# 📘 VibeTravels MVP – Product Requirements Summary

## 🧩 Product Overview
**Product Name:** VibeTravels (MVP) – Web Application  
**User Problem:** Difficulty in planning engaging trips based on loose notes  
**MVP Goal:** Enable users to save travel notes and convert them into personalized trip plans using AI, based on user preferences

---

## 🧠 Key Functional Requirements

### 🔐 Account Management
- Registration via email/password  
- Login functionality  
- Password reset excluded from MVP

### 👤 User Profile
- Preferences:
  - Budget (numeric)
  - Interests (select from: History, Art, Nature, Nightlife, Food, Beaches)
  - Travel Style (select from: Luxury, Budget, Adventure, All-inclusive, Touring)
  - Intensity (select from: Relaxed, Moderate, Intense)
- Prompt to fill out profile shown once after registration

### 📝 Note Management (CRUD)
- Create/Edit:
  - Title (max 100 characters)
  - Content (text, max 2000 characters)
  - Length validation
- View:
  - List displayed as cards with 150-character preview
  - Sort by creation/modification date
  - Pagination (10 per page)
- Delete:
  - Confirmation dialog
- Search: Excluded from MVP

### 🤖 AI Trip Plan Generation
- Triggered via button in note view
- Inputs:
  - Note (title + content)
  - Profile preferences
  - Start and end dates (required, end > start)
  - Budget (required, ≥ 0; budget entered during generation takes priority)
- Process:
  - Loading indicator shown
  - Generates 3 distinct plan variants (based on style, intensity, interests, location)
- Output:
  - 3 plan suggestions displayed as comparison cards
  - Each plan includes:
    - Title (AI-generated, editable before saving, max 100 characters, short and descriptive)
    - Content (list of places, suggested daily routes, attraction descriptions, general transport suggestions)
- AI Error Handling:
  - If generation fails, a wrapped message is shown asking user to clarify the note

### ✅ Plan Acceptance/Decline
- Interface:
  - Each card has edit icon for title and "Accept" / "Decline" buttons
- Acceptance:
  - User can accept one or more plans
  - Accepted cards change appearance (e.g., color), button disappears
  - Title editable before acceptance
- Decline:
  - User can decline individual suggestions or exit comparison view
- Saving:
  - Each accepted plan is saved as a separate entity
- Navigation:
  - After interaction, user is redirected to saved plans list

### 📂 Saved Plans Management (CRUD)
- View:
  - List displayed as cards with 150-character preview
  - Sort by creation/modification date
  - Pagination (10 per page)
- Edit:
  - Content editable in plain text format
- Delete:
  - Confirmation dialog

### 🔗 Relationships
- Notes and plans are separate entities  
- Deleting a note does not delete its generated plans  
- Editing a note does not affect existing plans  
- Profile changes do not affect already generated (but unaccepted) suggestions

### 📉 Limits
- No limits on number of notes or plans in MVP

### 📭 Empty States
- Defined messages for empty note and plan lists

### 📊 Success Criteria (Monitored in MVP)
- % of filled profiles and number of generated plans available in database  
- No dedicated user/admin interface for monitoring in MVP

### ⚙️ Non-Functional Requirements
- No specific non-functional requirements beyond basic validation and error handling

---

## ✅ Matched Recommendations

1. Defined user preferences and collection method (post-registration prompt)  
2. Specified structure and content of AI-generated plans and presentation format (3 cards)  
3. Established plan evaluation mechanism and user flow after acceptance  
4. Defined CRUD functionality for notes and plans, including character limits and validation  
5. Clarified relationship between notes and plans  
6. Listed interests, travel styles, and intensity options  
7. Designed AI error handling (wrapped message)  
8. Decided on editing format for saved plans (plain text)  
9. Defined title generation and editing mechanism  
10. Confirmed no impact of note/profile edits on existing/generated plans  
11. Implemented input validation (dates, budget, text length)  
12. Designed deletion confirmation dialogs and empty state messages  
13. Specified list presentation (cards with preview), sorting, and pagination  
14. Defined behavior for re-generating plans from the same note  
15. Added loading indicator during AI generation  
16. Specified rejection mechanism for AI suggestions

---

## 🚫 MVP Limitations

- No plan sharing  
- No multimedia support  
- No advanced logistics planning (flights, bookings)  
- No password reset  
- No note search  
- No rich text editor for plans (plain text only)  
- No active profile completion incentives (beyond one-time prompt)  
- No interface for success criteria tracking  
- No detailed feedback on rejected AI suggestions

---

## 🧭 Key User Stories / Usage Paths

1. **Registration & Onboarding:**  
   User registers, logs in, sees one-time prompt to fill profile, optionally sets preferences

2. **Note Creation & Management:**  
   User creates a new note (title + content), views list, edits or deletes notes

3. **Plan Generation:**  
   User clicks "Generate Plans" in note view, enters dates and budget, waits for AI results

4. **Plan Selection:**  
   User compares 3 suggestions (cards), edits titles, accepts or rejects, or exits view

5. **Plan Management:**  
   User views saved plans list, opens and edits plan content, deletes plans