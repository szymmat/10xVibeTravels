# Database Schema - VibeTravels (MVP)

## 1. Tables

### AspNetUsers
*   Provided by ASP.NET Core Identity.
*   **Primary Key:** `Id` (`nvarchar(450)`)

### Interests
*   Dictionary table for user interests.
*   Columns:
    *   `Id` (`uniqueidentifier`, PK, Default: `NEWID()`, Not Null)
    *   `Name` (`nvarchar(50)`, Unique, Not Null)

### TravelStyles
*   Dictionary table for travel styles.
*   Columns:
    *   `Id` (`uniqueidentifier`, PK, Default: `NEWID()`, Not Null)
    *   `Name` (`nvarchar(50)`, Unique, Not Null)

### Intensities
*   Dictionary table for travel intensities.
*   Columns:
    *   `Id` (`uniqueidentifier`, PK, Default: `NEWID()`, Not Null)
    *   `Name` (`nvarchar(50)`, Unique, Not Null)

### UserProfiles
*   Stores user preferences, linked 1-to-1 with `AspNetUsers`.
*   Columns:
    *   `Id` (`uniqueidentifier`, PK, Default: `NEWID()`, Not Null)
    *   `UserId` (`nvarchar(450)`, FK to `AspNetUsers(Id)`, Unique, Not Null, On Delete: Cascade)
    *   `Budget` (`decimal(9, 2)`, Null)
    *   `TravelStyleId` (`uniqueidentifier`, FK to `TravelStyles(Id)`, Null, On Delete: Set Null)
    *   `IntensityId` (`uniqueidentifier`, FK to `Intensities(Id)`, Null, On Delete: Set Null)
    *   `CreatedAt` (`datetime2(7)`, Default: `GETUTCDATE()`, Not Null)
    *   `ModifiedAt` (`datetime2(7)`, Default: `GETUTCDATE()`, Not Null)

### UserInterests
*   Junction table for the many-to-many relationship between `AspNetUsers` and `Interests`.
*   Columns:
    *   `UserId` (`nvarchar(450)`, Composite PK, FK to `AspNetUsers(Id)`, Not Null, On Delete: Cascade)
    *   `InterestId` (`uniqueidentifier`, Composite PK, FK to `Interests(Id)`, Not Null, On Delete: Cascade)

### Notes
*   Stores user's travel notes.
*   Columns:
    *   `Id` (`uniqueidentifier`, PK, Default: `NEWID()`, Not Null)
    *   `UserId` (`nvarchar(450)`, FK to `AspNetUsers(Id)`, Not Null, On Delete: Cascade)
    *   `Title` (`nvarchar(100)`, Not Null)
    *   `Content` (`nvarchar(2000)`, Not Null)
    *   `CreatedAt` (`datetime2(7)`, Default: `GETUTCDATE()`, Not Null)
    *   `ModifiedAt` (`datetime2(7)`, Default: `GETUTCDATE()`, Not Null)

### Plans
*   Stores AI-generated plan proposals and user-accepted plans.
*   Columns:
    *   `Id` (`uniqueidentifier`, PK, Default: `NEWID()`, Not Null)
    *   `UserId` (`nvarchar(450)`, FK to `AspNetUsers(Id)`, Not Null, On Delete: Cascade)
    *   `Status` (`nvarchar(20)`, Not Null, Check Constraint: `Status IN ('Generated', 'Accepted', 'Rejected')`)
    *   `StartDate` (`date`, Not Null)
    *   `EndDate` (`date`, Not Null)
    *   `Budget` (`decimal(9, 2)`, Not Null)
    *   `Title` (`nvarchar(100)`, Not Null)
    *   `Content` (`nvarchar(max)`, Not Null)
    *   `CreatedAt` (`datetime2(7)`, Default: `GETUTCDATE()`, Not Null)
    *   `ModifiedAt` (`datetime2(7)`, Default: `GETUTCDATE()`, Not Null)


## 2. Relationships

*   **UserProfiles <-> AspNetUsers:** One-to-One (enforced by `UNIQUE` constraint on `UserProfiles.UserId`). Deleting a user cascades to delete their profile.
*   **UserInterests (AspNetUsers <-> Interests):** Many-to-Many. Deleting a user or an interest cascades to delete the corresponding links in `UserInterests`.
*   **Notes <-> AspNetUsers:** Many-to-One. Multiple notes belong to one user. Deleting a user cascades to delete their notes.
*   **Plans <-> AspNetUsers:** Many-to-One. Multiple plans belong to one user. Deleting a user cascades to delete their plans.
*   **UserProfiles <-> TravelStyles:** Many-to-One (Optional). A profile can optionally link to one travel style. Deleting a travel style sets the `TravelStyleId` in `UserProfiles` to `NULL`.
*   **UserProfiles <-> Intensities:** Many-to-One (Optional). A profile can optionally link to one intensity. Deleting an intensity sets the `IntensityId` in `UserProfiles` to `NULL`.

## 3. Indexes

*   **Clustered:**
    *   Primary Keys in all tables (except `UserInterests` which has a composite PK).
*   **Non-Clustered:**
    *   `UserProfiles(UserId)` (Created automatically by the `UNIQUE` constraint)
    *   `UserInterests(InterestId)` (To optimize lookups by interest)
    *   `Notes(UserId)` (To optimize filtering notes by user)
    *   `Notes(ModifiedAt DESC)` (To optimize sorting notes list)
    *   `Notes(CreatedAt DESC)` (To optimize sorting notes list)
    *   `Plans(UserId)` (To optimize filtering plans by user)
    *   `Plans(Status)` (To optimize filtering plans by status)
    *   `Plans(ModifiedAt DESC)` (To optimize sorting plans list)
    *   `Plans(CreatedAt DESC)` (To optimize sorting plans list)

## 4. Additional Notes

*   **Naming Convention:** PascalCase for tables and columns.
*   **Primary Keys:** `uniqueidentifier` (GUID) with `NEWID()` default for all custom tables except the junction table `UserInterests`. `AspNetUsers` uses `nvarchar(450)` as per Identity standard.
*   **Timestamps:** `CreatedAt` and `ModifiedAt` use `datetime2(7)` with `GETUTCDATE()` default for tracking creation and modification times in UTC.
*   **Data Types:** Chosen based on requirements (e.g., `nvarchar` for text, `decimal(9, 2)` for budget assuming PLN, `date` for travel dates, `nvarchar(max)` for potentially long plan content).
*   **Constraints:** `UNIQUE` constraints ensure uniqueness where needed (dictionary names, `UserProfiles.UserId`). `CHECK` constraint enforces allowed values for `Plans.Status`. `NOT NULL` constraints enforce mandatory fields.
*   **Referential Integrity:** Foreign Keys with appropriate `ON DELETE` actions (Cascade for owned data, Set Null for optional references) maintain data consistency.
*   **Dictionary Data:** The `Interests`, `TravelStyles`, and `Intensities` tables will require initial data seeding based on the values specified in the PRD (Interests: Historia, Sztuka, Przyroda, Życie nocne, Jedzenie, Plaże; Travel Styles: Luksusowy, Budżetowy, Przygodowy; Intensities: Relaksacyjny, Umiarkowany, Intensywny).
*   **Budget Handling:** The `Budget` in `UserProfiles` is nullable. The `Budget` in `Plans` is not nullable, as it represents the specific budget used for generating *that* plan (either taken from the profile or provided during generation). Application logic must ensure a budget value is available before calling the AI generation.
*   **Plan Lifecycle:** Plans start with `Status = 'Generated'`. Users can change it to `'Accepted'` or `'Rejected'`. Only `'Accepted'` plans are considered "Saved Plans" for CRUD operations (editing content, deletion) from the user's perspective, although all statuses remain in the table as per planning decisions.
*   **Identity Tables:** The schema assumes the standard ASP.NET Core Identity tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, etc.) exist and are managed by the Identity framework. Only `AspNetUsers` is explicitly referenced for relationships. 