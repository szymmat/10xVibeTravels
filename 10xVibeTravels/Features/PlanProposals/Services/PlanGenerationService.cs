using _10xVibeTravels.Data;
using _10xVibeTravels.Features.PlanProposals.Exceptions;
using _10xVibeTravels.Features.PlanProposals.Requests;
using _10xVibeTravels.Features.PlanProposals.Responses;
using _10xVibeTravels.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;

namespace _10xVibeTravels.Features.PlanProposals.Services;

public class PlanGenerationService : IPlanGenerationService
{
    private readonly ApplicationDbContext _context;
    // UserManager removed as UserProfile is fetched via context and userId
    private readonly ILogger<PlanGenerationService> _logger;
    private readonly IOpenRouterService _openRouterService;

    public PlanGenerationService(
        ApplicationDbContext context,
        ILogger<PlanGenerationService> logger,
        IOpenRouterService openRouterService)
    {
        _context = context;
        _logger = logger;
        _openRouterService = openRouterService;
    }

    public async Task<List<PlanProposalResponse>> GeneratePlanProposalsAsync(GeneratePlanProposalRequest request, string userId)
    {
        _logger.LogInformation("Starting plan proposal generation for user {UserId} and note {NoteId}", userId, request.NoteId);

        // --- Step 5a: Fetch Note and UserProfile (including related data) ---
        var note = await _context.Notes
            .AsNoTracking() // Read-only for validation
            .FirstOrDefaultAsync(n => n.Id == request.NoteId);

        var userProfile = await _context.UserProfiles
            .Include(up => up.TravelStyle)
            .Include(up => up.Intensity)
            .Include(up => up.User)
            .Include(up => up.User!.UserInterests)
                .ThenInclude(ui => ui.Interest)
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        // Check if userProfile exists before querying interests
        if (userProfile == null) 
        {
            _logger.LogError("UserProfile not found for authenticated user {UserId}", userId);
            throw new ApplicationException($"UserProfile not found for user '{userId}'.");
        }

        // Fetch UserInterests separately
        var userInterests = await _context.UserInterests
            .Include(ui => ui.Interest)
            .Where(ui => ui.UserId == userProfile.UserId) 
            .AsNoTracking()
            .ToListAsync();

        // --- Step 5b: Validate note existence and ownership --- 
        if (note == null)
        {
            throw new NoteNotFoundException(request.NoteId);
        }
        if (note.UserId != userId)
        {
            _logger.LogWarning("Unauthorized access attempt: User {AttemptingUserId} tried to access note {NoteId} owned by {OwnerUserId}", userId, request.NoteId, note.UserId);
            throw new NoteForbiddenException(request.NoteId, userId);
        }

        // Validate dates
        if (request.EndDate <= request.StartDate)
        {
            throw new InvalidDateRangeException();
        }

        // --- Step 5c: Determine final budget --- 
        decimal finalBudget;
        if (request.Budget.HasValue)
        {
            finalBudget = request.Budget.Value;
        }
        else if (userProfile.Budget.HasValue)
        {
            finalBudget = userProfile.Budget.Value;
        }
        else
        {
            throw new BudgetNotAvailableException();
        }
        // Note: Range validation (>=0) is handled by model validation attributes

        // --- Step 5d: Construct AI prompt --- 
        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine("Generate 3 distinct travel plan proposals based on the following details:");
        promptBuilder.AppendLine("\n**Destination Idea & Notes:**");
        promptBuilder.AppendLine(note.Content); // Use the content of the source note
        promptBuilder.AppendLine("\n**Travel Dates:**");
        promptBuilder.AppendLine($"Start Date: {request.StartDate:yyyy-MM-dd}");
        promptBuilder.AppendLine($"End Date: {request.EndDate:yyyy-MM-dd}");
        promptBuilder.AppendLine("\n**Budget:**");
        promptBuilder.AppendLine(finalBudget.ToString("C")); // Format as currency
        promptBuilder.AppendLine("\n**User Preferences:**");
        if (userProfile.TravelStyle != null)
            promptBuilder.AppendLine($"- Travel Style: {userProfile.TravelStyle.Name}");
        if (userProfile.Intensity != null)
            promptBuilder.AppendLine($"- Intensity: {userProfile.Intensity.Name}");
        var interests = userInterests.Select(ui => ui.Interest?.Name).ToList();
        if (interests.Any())
            promptBuilder.AppendLine($"- Interests: {string.Join(", ", interests)}");
        else 
            promptBuilder.AppendLine("- Interests: Not specified");
        
        promptBuilder.AppendLine("\nPlease provide each proposal with a clear Title and detailed Content (e.g., day-by-day itinerary).");
        string prompt = promptBuilder.ToString();
        _logger.LogDebug("Generated AI Prompt for user {UserId}:\n{Prompt}", userId, prompt); // Log prompt for debugging if needed

        // --- Step 5e: Call external AI service (IOpenRouterService) --- 
        List<GeneratedPlanDto> generatedPlans;
        try
        {
            generatedPlans = await _openRouterService.GeneratePlanProposalsAsync(prompt);
            if (generatedPlans == null || generatedPlans.Count != 3) 
            {
                 _logger.LogWarning("AI service returned an unexpected number of plans ({Count}) for user {UserId}", generatedPlans?.Count ?? 0, userId);
                 // Decide if this is a hard fail or if partial results are acceptable
                 throw new AiServiceException("AI service did not return exactly 3 plan proposals.");
            }
        }
        catch (Exception ex) // Catch broader exceptions from the service call
        {
            _logger.LogError(ex, "Error calling AI service for user {UserId}", userId);
            throw new AiServiceException("Failed to generate plans due to an AI service error.", ex);
        }

        // --- Step 5g: Create 3 Plan entities --- 
        var newPlans = new List<Plan>();
        var now = DateTime.UtcNow;
        foreach (var generatedPlan in generatedPlans)
        {
            var plan = new Plan
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StartDate = request.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = request.EndDate.ToDateTime(TimeOnly.MinValue),
                Budget = finalBudget,
                Title = generatedPlan.Title,
                Content = generatedPlan.Content,
                Status = PlanStatus.Generated.ToString(), // Assuming PlanStatus enum or constant exists
                CreatedAt = now,
                ModifiedAt = now
            };
            newPlans.Add(plan);
        }

        // --- Step 5h: Save Plan entities --- 
        try
        {
            _context.Plans.AddRange(newPlans);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully saved {Count} new plan proposals for user {UserId}", newPlans.Count, userId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error saving plan proposals for user {UserId}", userId);
            // Throw a general server error exception or a specific DB error exception
            throw new ApplicationException("An error occurred while saving the plan proposals.", ex);
        }

        // --- Step 5i: Map Plan entities to PlanProposalResponse DTOs --- 
        var responseProposals = newPlans.Select(p => new PlanProposalResponse
        {
            Id = p.Id,
            Status = p.Status.ToString(),
            StartDate = DateOnly.FromDateTime(p.StartDate),
            EndDate = DateOnly.FromDateTime(p.EndDate),
            Budget = p.Budget,
            Title = p.Title,
            Content = p.Content,
            CreatedAt = p.CreatedAt,
            ModifiedAt = p.ModifiedAt
        }).ToList();

        _logger.LogInformation("Successfully generated and mapped {Count} plan proposals for user {UserId}", responseProposals.Count, userId);
        return responseProposals;
    }
} 