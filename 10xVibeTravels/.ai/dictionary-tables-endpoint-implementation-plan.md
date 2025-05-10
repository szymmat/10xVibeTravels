# API Endpoint Implementation Plan: Dictionary Endpoints

This plan covers the implementation of the following dictionary endpoints:
*   `GET /interests`
*   `GET /travel-styles`
*   `GET /intensities`

## 1. Endpoint Overview
These endpoints provide read-only access to the predefined dictionary data used throughout the application (Interests, Travel Styles, and Intensities). They allow the frontend or other clients to fetch the available options for user profiles, plan generation criteria, etc.

## 2. Request Details
*   **HTTP Method:** `GET`
*   **URL Structure:**
    *   `/interests`
    *   `/travel-styles`
    *   `/intensities`
*   **Parameters:**
    *   Required: None
    *   Optional: None
*   **Request Body:** None

## 3. Utilized Types (DTOs)
The following Data Transfer Objects (DTOs) will be used for the responses. They should be created if they don't already exist, likely in a `Dtos` or `Contracts` folder.

*   **InterestDto:**
    ```csharp
    public class InterestDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    ```
*   **TravelStyleDto:**
    ```csharp
    public class TravelStyleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    ```
*   **IntensityDto:**
    ```csharp
    public class IntensityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
    ```

## 4. Response Details
*   **Success Response:**
    *   Code: `200 OK`
    *   Body (`/interests`): `List<InterestDto>` (JSON array of interest objects)
    *   Body (`/travel-styles`): `List<TravelStyleDto>` (JSON array of travel style objects)
    *   Body (`/intensities`): `List<IntensityDto>` (JSON array of intensity objects)
    *   Example Body (`/interests`):
        ```json
        [
          { "id": "guid", "name": "Historia" },
          { "id": "guid", "name": "Sztuka" },
          // ...
        ]
        ```
*   **Error Response:**
    *   Code: `500 Internal Server Error`
    *   Body: Standard ASP.NET Core error response format.

## 5. Data Flow
1.  Client sends a `GET` request to `/interests`, `/travel-styles`, or `/intensities`.
2.  ASP.NET Core routing directs the request to the corresponding action method in a dedicated `DictionaryController` (or potentially separate controllers like `InterestsController`, etc.).
3.  The controller action method calls the appropriate application service method (e.g., `InterestService.GetAllAsync()`, `TravelStyleService.GetAllAsync()`, `IntensityService.GetAllAsync()`).
4.  The service method uses the `ApplicationDbContext` (Entity Framework Core) to query the respective database table (`Interests`, `TravelStyles`, or `Intensities`).
5.  EF Core retrieves all records from the table.
6.  The service method maps the retrieved entity objects (e.g., `Interest`) to their corresponding DTOs (e.g., `InterestDto`). Libraries like AutoMapper can be used for this.
7.  The service returns the list of DTOs to the controller.
8.  The controller returns an `OkObjectResult` (resulting in a `200 OK` status code) with the list of DTOs as the response body, serialized to JSON.
9.  If any error occurs during database access or processing within the service/controller, the standard ASP.NET Core exception handling middleware catches it and returns a `500 Internal Server Error` response.

## 6. Security Considerations
*   **Authentication/Authorization:** These endpoints are currently planned as publicly accessible. No specific authentication or authorization is required. If requirements change, implement standard ASP.NET Core Identity mechanisms (e.g., `[Authorize]` attribute).
*   **Input Validation:** Not applicable as there are no inputs.

## 7. Error Handling
*   **Database Errors:** Any `DbUpdateException`, `SqlException`, or other database-related exceptions during the EF Core query execution will result in a `500 Internal Server Error`. Ensure standard ASP.NET Core logging is configured to capture these exceptions for debugging.
*   **Mapping Errors:** Errors during the mapping from Entities to DTOs (if using AutoMapper or similar) should also result in a `500 Internal Server Error` and be logged.

## 8. Performance Considerations
*   **Database Query:** The primary operation is fetching all records from relatively small dictionary tables. Performance impact should be minimal.
*   **Caching:** For dictionary data that changes infrequently, consider implementing caching (e.g., in-memory caching via `IMemoryCache` or distributed caching) at the service layer to reduce database load and improve response times. Add appropriate cache invalidation if the dictionary data can be modified through other means (though not planned via API currently).

## 9. Implementation Steps
1.  **Create DTOs:** Define `InterestDto`, `TravelStyleDto`, and `IntensityDto` in a shared contracts or DTO project/folder.
2.  **Create Services:**
    *   Define interfaces (`IInterestService`, `ITravelStyleService`, `IIntensityService`) in an Application Core/Interfaces project/folder.
    *   Implement these services (`InterestService`, `TravelStyleService`, `IntensityService`) in the Infrastructure or Application project/folder.
    *   Inject `ApplicationDbContext` into the service implementations.
    *   Implement `GetAllAsync` methods in each service to query the database using EF Core (e.g., `_context.Interests.ToListAsync()`) and map entities to DTOs (consider using AutoMapper for mapping).
3.  **Configure Dependency Injection:** Register the services and their interfaces in `Program.cs` or `Startup.cs` (e.g., `builder.Services.AddScoped<IInterestService, InterestService>();`). Configure AutoMapper if used.
4.  **Create Controller:**
    *   Create a new API Controller (e.g., `DictionaryController` or separate controllers like `InterestsController`, `TravelStylesController`, `IntensitiesController`).
    *   Inject the required service interfaces into the controller's constructor.
5.  **Implement Controller Actions:**
    *   Create `[HttpGet("/interests")]` action, call `_interestService.GetAllAsync()`, and return `Ok(result)`.
    *   Create `[HttpGet("/travel-styles")]` action, call `_travelStyleService.GetAllAsync()`, and return `Ok(result)`.
    *   Create `[HttpGet("/intensities")]` action, call `_intensityService.GetAllAsync()`, and return `Ok(result)`.
    *   Add `[ProducesResponseType(typeof(List<InterestDto>), StatusCodes.Status200OK)]`, `[ProducesResponseType(StatusCodes.Status500InternalServerError)]` attributes (and similarly for other DTOs) to actions for Swagger/OpenAPI documentation.