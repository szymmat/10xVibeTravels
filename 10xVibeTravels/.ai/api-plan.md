# REST API Plan

## 1. Resources

*   **UserProfile**: Represents user preferences (Budget, Travel Style, Intensity, Interests). Maps primarily to `UserProfiles` and `UserInterests` tables. Linked 1-to-1 with the authenticated user (`AspNetUsers`).
*   **Interest**: Represents a selectable interest category. Maps to `Interests` table (Read-only dictionary).
*   **TravelStyle**: Represents a selectable travel style. Maps to `TravelStyles` table (Read-only dictionary).
*   **Intensity**: Represents a selectable travel intensity. Maps to `Intensities` table (Read-only dictionary).
*   **Note**: Represents user's travel notes. Maps to `Notes` table.
*   **Plan**: Represents AI-generated or user-saved travel plans. Maps to `Plans` table. Includes generated proposals, accepted plans, and rejected plans, distinguished by `Status`.
*   **PlanProposal**: A conceptual resource representing the AI generation process and its output (3 `Plan` entities with `Status='Generated'`).

## 2. Endpoints

Base Path: `/api/v1`

### 2.1 User Profile

*   **Get User Profile**
    *   **Method:** `GET`
    *   **URL:** `/profile`
    *   **Description:** Retrieves the profile preferences for the authenticated user.
    *   **Query Parameters:** None
    *   **Request Payload:** None
    *   **Response Payload:**
        ```json
        {
          "budget": 999.99, // decimal or null
          "travelStyle": {
            "id": "guid",
            "name": "Luksusowy"
          }, // object or null
          "intensity": {
            "id": "guid",
            "name": "Umiarkowany"
          }, // object or null
          "interests": [
            {
              "id": "guid",
              "name": "Historia"
            },
            {
              "id": "guid",
              "name": "Sztuka"
            }
            // ...
          ], // array of objects
          "createdAt": "datetime",
          "modifiedAt": "datetime"
        }
        ```
    *   **Success:** `200 OK`
    *   **Errors:** `401 Unauthorized`, `404 Not Found` (if profile doesn't exist - though it might be auto-created), `500 Internal Server Error`

*   **Update User Profile**
    *   **Method:** `PATCH`
    *   **URL:** `/profile`
    *   **Description:** Updates specific preferences (Budget, Travel Style, Intensity) for the authenticated user. Creates the profile if it doesn't exist.
    *   **Query Parameters:** None
    *   **Request Payload:**
        ```json
        {
          "budget": 1500.00, // optional, decimal or null
          "travelStyleId": "guid", // optional, guid or null
          "intensityId": "guid" // optional, guid or null
        }
        ```
    *   **Response Payload:** (Updated User Profile - same as GET response)
    *   **Success:** `200 OK`
    *   **Errors:** `400 Bad Request` (Invalid input/IDs), `401 Unauthorized`, `500 Internal Server Error`

*   **Set User Interests**
    *   **Method:** `PUT`
    *   **URL:** `/profile/interests`
    *   **Description:** Replaces the entire list of interests for the authenticated user. An empty array clears interests.
    *   **Query Parameters:** None
    *   **Request Payload:**
        ```json
        {
          "interestIds": [
            "guid1",
            "guid2"
            // ...
          ] // Array of Interest IDs (uniqueidentifier)
        }
        ```
    *   **Response Payload:** (Updated list of User Interests)
        ```json
        [
          {
            "id": "guid1",
            "name": "Historia"
          },
          {
            "id": "guid2",
            "name": "Sztuka"
          }
          // ...
        ]
        ```
    *   **Success:** `200 OK`
    *   **Errors:** `400 Bad Request` (Invalid input/IDs), `401 Unauthorized`, `500 Internal Server Error`

### 2.2 Dictionaries (Interests, Travel Styles, Intensities)

*   **Get All Interests**
    *   **Method:** `GET`
    *   **URL:** `/interests`
    *   **Description:** Retrieves the list of all available interests.
    *   **Query Parameters:** None
    *   **Request Payload:** None
    *   **Response Payload:**
        ```json
        [
          {
            "id": "guid",
            "name": "Historia"
          },
          {
            "id": "guid",
            "name": "Sztuka"
          }
          // ...
        ]
        ```
    *   **Success:** `200 OK`
    *   **Errors:** `500 Internal Server Error`

*   **Get All Travel Styles**
    *   **Method:** `GET`
    *   **URL:** `/travel-styles`
    *   **Description:** Retrieves the list of all available travel styles.
    *   **Query Parameters:** None
    *   **Request Payload:** None
    *   **Response Payload:**
        ```json
        [
          {
            "id": "guid",
            "name": "Luksusowy"
          },
          {
            "id": "guid",
            "name": "Budżetowy"
          }
          // ...
        ]
        ```
    *   **Success:** `200 OK`
    *   **Errors:** `500 Internal Server Error`

*   **Get All Intensities**
    *   **Method:** `GET`
    *   **URL:** `/intensities`
    *   **Description:** Retrieves the list of all available travel intensities.
    *   **Query Parameters:** None
    *   **Request Payload:** None
    *   **Response Payload:**
        ```json
        [
          {
            "id": "guid",
            "name": "Relaksacyjny"
          },
          {
            "id": "guid",
            "name": "Umiarkowany"
          }
          // ...
        ]
        ```
    *   **Success:** `200 OK`
    *   **Errors:** `500 Internal Server Error`

### 2.3 Notes

*   **Get Notes List**
    *   **Method:** `GET`
    *   **URL:** `/notes`
    *   **Description:** Retrieves a paginated list of notes for the authenticated user.
    *   **Query Parameters:**
        *   `page` (int, default: 1): Page number.
        *   `pageSize` (int, default: 10): Number of items per page.
        *   `sortBy` (string, default: "modifiedAt"): Field to sort by (`createdAt` or `modifiedAt`).
        *   `sortDirection` (string, default: "desc"): Sort direction (`asc` or `desc`).
    *   **Request Payload:** None
    *   **Response Payload:**
        ```json
        {
          "items": [
            {
              "id": "guid",
              "title": "Note Title",
              "contentPreview": "First 150 chars of content...", // Generated by API
              "createdAt": "datetime",
              "modifiedAt": "datetime"
            }
            // ...
          ],
          "page": 1,
          "pageSize": 10,
          "totalItems": 50,
          "totalPages": 5
        }
        ```
    *   **Success:** `200 OK`
    *   **Errors:** `400 Bad Request` (Invalid query params), `401 Unauthorized`, `500 Internal Server Error`

*   **Create Note**
    *   **Method:** `POST`
    *   **URL:** `/notes`
    *   **Description:** Creates a new note for the authenticated user.
    *   **Query Parameters:** None
    *   **Request Payload:**
        ```json
        {
          "title": "New Note Title", // Required, max 100 chars
          "content": "Note content details..." // Required, max 2000 chars
        }
        ```
    *   **Response Payload:** (The created Note, including ID and timestamps)
        ```json
        {
          "id": "guid",
          "title": "New Note Title",
          "content": "Note content details...",
          "createdAt": "datetime",
          "modifiedAt": "datetime"
        }
        ```
    *   **Success:** `201 Created` (with `Location` header: `/api/v1/notes/{id}`)
    *   **Errors:** `400 Bad Request` (Validation errors), `401 Unauthorized`, `500 Internal Server Error`

*   **Get Note Details**
    *   **Method:** `GET`
    *   **URL:** `/notes/{id}`
    *   **Description:** Retrieves the full details of a specific note.
    *   **Path Parameters:** `id` (guid): ID of the note.
    *   **Query Parameters:** None
    *   **Request Payload:** None
    *   **Response Payload:**
        ```json
        {
          "id": "guid",
          "title": "Note Title",
          "content": "Full note content...",
          "createdAt": "datetime",
          "modifiedAt": "datetime"
        }
        ```
    *   **Success:** `200 OK`
    *   **Errors:** `401 Unauthorized`, `403 Forbidden` (Not user's note), `404 Not Found`, `500 Internal Server Error`

*   **Update Note**
    *   **Method:** `PUT`
    *   **URL:** `/notes/{id}`
    *   **Description:** Updates the title and content of an existing note.
    *   **Path Parameters:** `id` (guid): ID of the note.
    *   **Query Parameters:** None
    *   **Request Payload:**
        ```json
        {
          "title": "Updated Note Title", // Required, max 100 chars
          "content": "Updated note content..." // Required, max 2000 chars
        }
        ```
    *   **Response Payload:** (The updated Note)
    *   **Success:** `200 OK`
    *   **Errors:** `400 Bad Request` (Validation errors), `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`

*   **Delete Note**
    *   **Method:** `DELETE`
    *   **URL:** `/notes/{id}`
    *   **Description:** Deletes a specific note.
    *   **Path Parameters:** `id` (guid): ID of the note.
    *   **Query Parameters:** None
    *   **Request Payload:** None
    *   **Response Payload:** None
    *   **Success:** `204 No Content`
    *   **Errors:** `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`

### 2.4 Plan Proposals (AI Generation)

*   **Generate Plan Proposals**
    *   **Method:** `POST`
    *   **URL:** `/plan-proposals`
    *   **Description:** Initiates AI generation of 3 travel plan proposals based on a note, user profile, and specific inputs. Creates 3 `Plan` entities with `Status='Generated'`.
    *   **Query Parameters:** None
    *   **Request Payload:**
        ```json
        {
          "noteId": "guid", // ID of the source note
          "startDate": "YYYY-MM-DD", // Required
          "endDate": "YYYY-MM-DD", // Required, must be after startDate
          "budget": 1200.50 // Optional, overrides profile budget if provided. Must be >= 0.
        }
        ```
    *   **Response Payload:** (Array containing the 3 generated proposals)
        ```json
        [
          {
            "id": "guid1", // Plan ID
            "status": "Generated",
            "startDate": "YYYY-MM-DD",
            "endDate": "YYYY-MM-DD",
            "budget": 1200.50,
            "title": "AI Generated Title 1", // Editable by user later
            "content": "AI generated plan content 1...",
            "createdAt": "datetime",
            "modifiedAt": "datetime"
          },
          {
            "id": "guid2", // Plan ID
            // ... proposal 2 ...
          },
          {
            "id": "guid3", // Plan ID
            // ... proposal 3 ...
          }
        ]
        ```
    *   **Success:** `201 Created`
    *   **Errors:** `400 Bad Request` (Validation errors, Invalid noteId), `401 Unauthorized`, `403 Forbidden` (Note not owned by user), `404 Not Found` (Note not found), `500 Internal Server Error`, `503 Service Unavailable` (AI service error/timeout - include error details if possible as per PRD).

### 2.5 Plans (Generated, Accepted, Rejected)

*   **Get Plans List**
    *   **Method:** `GET`
    *   **URL:** `/plans`
    *   **Description:** Retrieves a paginated list of plans for the authenticated user, filterable by status. Used for viewing 'Generated' proposals or 'Accepted' (saved) plans.
    *   **Query Parameters:**
        *   `status` (string, optional): Filter by status (`Generated`, `Accepted`, `Rejected`). Default: `Accepted`.
        *   `page` (int, default: 1): Page number.
        *   `pageSize` (int, default: 10): Number of items per page.
        *   `sortBy` (string, default: "modifiedAt"): Field to sort by (`createdAt` or `modifiedAt`).
        *   `sortDirection` (string, default: "desc"): Sort direction (`asc` or `desc`).
    *   **Request Payload:** None
    *   **Response Payload:**
        ```json
        {
          "items": [
            {
              "id": "guid",
              "status": "Accepted", // or Generated, Rejected
              "title": "Plan Title",
              "contentPreview": "First 150 chars of content...", // Generated by API
              "startDate": "YYYY-MM-DD",
              "endDate": "YYYY-MM-DD",
              "budget": 1200.50,
              "createdAt": "datetime",
              "modifiedAt": "datetime"
            }
            // ...
          ],
          "page": 1,
          "pageSize": 10,
          "totalItems": 25,
          "totalPages": 3
        }
        ```
    *   **Success:** `200 OK`
    *   **Errors:** `400 Bad Request` (Invalid query params), `401 Unauthorized`, `500 Internal Server Error`

*   **Get Plan Details**
    *   **Method:** `GET`
    *   **URL:** `/plans/{id}`
    *   **Description:** Retrieves the full details of a specific plan (proposal or saved).
    *   **Path Parameters:** `id` (guid): ID of the plan.
    *   **Query Parameters:** None
    *   **Request Payload:** None
    *   **Response Payload:**
        ```json
        {
          "id": "guid",
          "status": "Accepted", // or Generated, Rejected
          "title": "Plan Title",
          "content": "Full plan content...",
          "startDate": "YYYY-MM-DD",
          "endDate": "YYYY-MM-DD",
          "budget": 1200.50,
          "createdAt": "datetime",
          "modifiedAt": "datetime"
        }
        ```
    *   **Success:** `200 OK`
    *   **Errors:** `401 Unauthorized`, `403 Forbidden` (Not user's plan), `404 Not Found`, `500 Internal Server Error`

*   **Update Plan (Proposal Title or Status, Saved Plan Content)**
    *   **Method:** `PATCH`
    *   **URL:** `/plans/{id}`
    *   **Description:** Updates a plan. Allows changing Title or Status for 'Generated' plans, or Content for 'Accepted' plans. Status can be changed from 'Generated' to 'Accepted' or 'Rejected'.
    *   **Path Parameters:** `id` (guid): ID of the plan.
    *   **Query Parameters:** None
    *   **Request Payload (Example: Accept Proposal):**
        ```json
        {
          "status": "Accepted"
        }
        ```
    *   **Request Payload (Example: Update Proposal Title):**
        ```json
        {
          "title": "My Custom Plan Title" // Max 100 chars
        }
        ```
    *   **Request Payload (Example: Update Saved Plan Content):**
        ```json
        {
          "content": "Updated plan details..." // Plain text
        }
        ```
    *   **Response Payload:** (The updated Plan)
    *   **Success:** `200 OK`
    *   **Errors:** `400 Bad Request` (Validation errors, invalid status transition, attempt to modify immutable field), `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`

*   **Delete Plan**
    *   **Method:** `DELETE`
    *   **URL:** `/plans/{id}`
    *   **Description:** Deletes a specific plan (typically only 'Accepted' or 'Rejected' ones from user perspective, but API might allow deleting 'Generated' ones too).
    *   **Path Parameters:** `id` (guid): ID of the plan.
    *   **Query Parameters:** None
    *   **Request Payload:** None
    *   **Response Payload:** None
    *   **Success:** `204 No Content`
    *   **Errors:** `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `500 Internal Server Error`

## 3. Authentication and Authorization

*   **Mechanism:** ASP.NET Core Identity handles user registration and login. The API expects JWT Bearer tokens issued by the Identity system to be included in the `Authorization` header for protected endpoints.
*   **Implementation:**
    *   Most endpoints (Profile, Notes, Plans, Plan Proposals) require authentication. This will be enforced using the `[Authorize]` attribute in ASP.NET Core controllers.
    *   Endpoints for dictionary data (`/interests`, `/travel-styles`, `/intensities`) may be public (`[AllowAnonymous]`) if desired, or require authentication. Assume authenticated for now.
    *   Authorization logic within endpoint handlers must ensure users can only access or modify their own data (e.g., check `note.UserId == currentUser.Id`). Attempts to access other users' data result in `403 Forbidden` or `404 Not Found`.

## 4. Validation and Business Logic

*   **Input Validation:**
    *   Implemented using Data Annotations on request DTOs and/or FluentValidation within the API handlers.
    *   Checks include: `Required`, `MaxLength`, `MinLength`, `Range`, valid `enum`/string values (Plan Status), valid date formats, `EndDate > StartDate`, numeric types, valid GUIDs for FKs.
    *   Validation failures result in `400 Bad Request` with details.
    *   Specific limits: Note Title (100), Note Content (2000), Plan Title (100), Budget (>=0).
*   **Business Logic Implementation:**
    *   **Profile:** `PATCH /profile` creates or updates `UserProfiles`. `PUT /profile/interests` manages the `UserInterests` junction table.
    *   **Notes/Plans CRUD:** Standard data access operations on `Notes` and `Plans` tables, respecting user ownership. Pagination/sorting logic applied at the query level, utilizing DB indexes.
    *   **AI Plan Generation (`POST /plan-proposals`):**
        1.  Validate input (`noteId`, dates, budget).
        2.  Retrieve source `Note` content and `UserProfile` preferences for the authenticated user.
        3.  Construct the prompt/request for the AI service (OpenRouter).
        4.  Call the AI service.
        5.  Handle AI response: If successful, parse the 3 plan variants. If failed, return `503 Service Unavailable` with details.
        6.  Create 3 `Plan` entities in the database with `Status='Generated'`, linking them to the user and storing the generated content, dates, budget.
        7.  Return the created `Plan` entities.
    *   **Plan Interaction (`PATCH /plans/{id}`):**
        1.  Verify plan exists and belongs to the user.
        2.  If updating status: Check if current status is 'Generated' and target is 'Accepted' or 'Rejected'. Update `Status`.
        3.  If updating title: Check if current status is 'Generated'. Update `Title`.
        4.  If updating content: Check if current status is 'Accepted'. Update `Content`.
        5.  Return updated plan.
    *   **Ownership:** All operations modifying or deleting data must verify the resource belongs to the authenticated user.
    *   **Data Integrity:** Relies on DB constraints (FKs, UNIQUE, CHECK) and API validation. Cascade deletes defined in the schema handle related data removal (e.g., User deletion).
    *   **Rate Limiting:** (Recommended) Implement using ASP.NET Core middleware, particularly for the `/plan-proposals` endpoint.
``` 