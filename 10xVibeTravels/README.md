# VibeTravels (MVP)

[![Project Status: MVP](https://img.shields.io/badge/status-MVP-orange.svg)](./.ai/prd.md)

VibeTravels is a web application designed to help users easily plan engaging trips by transforming their unstructured travel notes into personalized, detailed itineraries using AI.

## Table of Contents

- [Project Description](#project-description)
- [Tech Stack](#tech-stack)
- [Getting Started Locally](#getting-started-locally)
- [Available Scripts](#available-scripts)
- [Project Scope (MVP)](#project-scope-mvp)
- [Project Status](#project-status)
- [License](#license)

## Project Description

Planning interesting and personalized trips can be time-consuming. Users often start with scattered ideas and notes, but turning them into a concrete plan is challenging. VibeTravels aims to solve this by leveraging AI to process user notes and preferences (like budget, interests, travel style, and desired intensity) to generate tailored travel plan suggestions. The Minimum Viable Product (MVP) focuses on core features like note management, user profiles, AI plan generation, and managing saved itineraries.

For detailed requirements, see the [Product Requirements Document](./.ai/prd.md).

## Tech Stack

-   **Frontend:**
    -   Blazor Server
    -   Bootstrap
-   **Backend:**
    -   ASP.NET Core 8.0
    -   Entity Framework Core 8
    -   Microsoft SQL Server
    -   ASP.NET Core Identity (for user management)
-   **AI Integration:**
    -   [Openrouter.ai](https://openrouter.ai/) (for accessing various LLMs)
-   **CI/CD:**
    -   GitHub Actions
-   **Hosting:**
    -   DigitalOcean (via Docker)

## Getting Started Locally

### Prerequisites

-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [Microsoft SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (Express, Developer, or other editions)
-   An API Key from [Openrouter.ai](https://openrouter.ai/) (Optional, for AI features)

### Setup

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-username/10xVibeTravels.git
    cd 10xVibeTravels
    ```
2.  **Configure Database Connection:**
    -   Open `appsettings.Development.json`.
    -   Modify the `DefaultConnection` string to point to your local SQL Server instance.
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=VibeTravels;Trusted_Connection=True;MultipleActiveResultSets=true"
      },
      // ... other settings
    }
    ```
3.  **Configure API Keys (Optional):**
    -   It's recommended to use .NET User Secrets to store sensitive information like API keys.
    -   Initialize User Secrets for the project (if not already done):
        ```bash
        dotnet user-secrets init
        ```
    -   Set the Openrouter API key (replace `<your_api_key>`):
        ```bash
        dotnet user-secrets set "OpenrouterApiKey" "<your_api_key>"
        ```
4.  **Apply Database Migrations:**
    -   Ensure Entity Framework Core tools are installed or install them globally:
        ```bash
        dotnet tool install --global dotnet-ef
        ```
    -   Run the following command in the project directory (where the `.csproj` file is located):
        ```bash
        dotnet ef database update
        ```
5.  **Run the Application:**
    ```bash
    dotnet run
    ```
    Or use `dotnet watch run` for hot reload during development.

The application should now be running locally, typically at `https://localhost:7xxx` and `http://localhost:5xxx`.

## Available Scripts

Standard .NET CLI commands can be used within the project directory:

-   `dotnet build`: Compiles the project.
-   `dotnet run`: Builds and runs the application.
-   `dotnet watch run`: Runs the application with hot reload enabled.
-   `dotnet test`: Executes unit/integration tests (if any are configured).
-   `dotnet ef migrations add <MigrationName>`: Creates a new EF Core database migration.
-   `dotnet ef database update`: Applies pending EF Core migrations to the database.
-   `dotnet clean`: Cleans build outputs.

## Project Scope (MVP)

The current focus is on delivering the Minimum Viable Product (MVP) which includes:

**Included Features:**

-   User account management (Email/Password registration & login).
-   User profile with travel preferences (Budget, Interests, Style, Intensity).
-   Note Management (CRUD operations for travel notes).
-   AI-powered generation of 3 distinct travel plan proposals based on a note, user preferences, dates, and budget.
-   Interaction with AI proposals (Edit title, Accept, Reject).
-   Saved Plan Management (CRUD operations for accepted plans, plain text editing of content).
-   Basic data validation and user feedback (loading indicators, empty states, error messages).

Refer to the [PRD](./.ai/prd.md) for full details.

## Project Status

**Active Development (MVP Stage)**

This project is currently under active development, focusing on implementing the core features defined for the Minimum Viable Product.

## License

The project is licensed under MIT License.