using _10xVibeTravels.Data;
using _10xVibeTravels.Interfaces;
using _10xVibeTravels.Exceptions;
using Microsoft.EntityFrameworkCore;
using _10xVibeTravels.Requests;
using _10xVibeTravels.Responses;
using System.Text;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Models;

namespace _10xVibeTravels.Services;

public class PlanGenerationService : IPlanGenerationService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly ILogger<PlanGenerationService> _logger;
    private readonly IOpenRouterService _openRouterService;

    public PlanGenerationService(
        IDbContextFactory<ApplicationDbContext> contextFactory,
        ILogger<PlanGenerationService> logger,
        IOpenRouterService openRouterService)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _openRouterService = openRouterService;
    }

    public async Task<List<PlanProposalResponse>> GeneratePlanProposalsAsync(GeneratePlanProposalRequest request, string userId)
    {
        _logger.LogInformation("Starting plan proposal generation for user {UserId} and note {NoteId}", userId, request.NoteId);

        // --- Step 5a: Fetch Note and UserProfile (including related data) ---
        await using var context = await _contextFactory.CreateDbContextAsync();
        var note = await context.Notes
            .AsNoTracking() // Read-only for validation
            .FirstOrDefaultAsync(n => n.Id == request.NoteId);

        var userProfile = await context.UserProfiles
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
        var userInterests = await context.UserInterests
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

        // --- Step 5d: Construct AI prompt (user message part) --- 
        var userMessageBuilder = new StringBuilder();
        userMessageBuilder.AppendLine("**Destination Idea & Notes:**");
        userMessageBuilder.AppendLine(note.Content);
        userMessageBuilder.AppendLine("\n**Travel Dates:**");
        userMessageBuilder.AppendLine($"Start Date: {request.StartDate:yyyy-MM-dd}");
        userMessageBuilder.AppendLine($"End Date: {request.EndDate:yyyy-MM-dd}");
        userMessageBuilder.AppendLine("\n**Budget:**");
        userMessageBuilder.AppendLine(finalBudget.ToString("C"));
        userMessageBuilder.AppendLine("\n**User Preferences:**");
        if (userProfile.TravelStyle != null)
            userMessageBuilder.AppendLine($"- Travel Style: {userProfile.TravelStyle.Name}");
        if (userProfile.Intensity != null)
            userMessageBuilder.AppendLine($"- Intensity: {userProfile.Intensity.Name}");
        var interests = userInterests.Select(ui => ui.Interest?.Name).Where(name => !string.IsNullOrEmpty(name)).ToList();
        if (interests.Any())
            userMessageBuilder.AppendLine($"- Interests: {string.Join(", ", interests)}");
        else 
            userMessageBuilder.AppendLine("- Interests: Not specified");
        
        string userMessage = userMessageBuilder.ToString();
        _logger.LogDebug("Generated AI User Message for user {UserId}:\n{UserMessage}", userId, userMessage);

        // --- Define System Message and Response Format --- 
        string systemMessage = "You are a travel planning assistant. " +
                             "Your task is to generate exactly 3 distinct travel plan proposals based on the user's details. " +
                             "Each proposal must be a JSON object with a \"Title\" (string) and \"Content\" (string, e.g., a concise day-by-day itinerary or thematic overview). " +
                             "The final output must be a valid JSON array containing these 3 proposal objects. Do not include any other text or explanations outside the JSON array.";

        var responseFormat = new ResponseFormat(
            Type: "json_schema",
            json_schema: new Models.JsonSchema
            {
                Name = "travelPlanResponse",
                Schema = new {
                    type = "object",
                    properties = new {
                        items = new {
                            type = "array",
                            minItems = 3,
                            maxItems = 3,
                            items = new {
                                type = "object",
                                required = new[] { "title", "content" },
                                properties = new {
                                    title = new { type = "string" },
                                    content = new { type = "string" }
                                },
                                additionalProperties = false
                            }
                        }
                    },
                    required = new[] { "items" },
                    additionalProperties = false
                }
            }
        );

        // --- Step 5e: Call external AI service (IOpenRouterService) --- 
        TravelPlanAIResponse? aiServiceResponse;
        List<_10xVibeTravels.Dtos.GeneratedPlanDto>? generatedPlans = null;
        try
        {
            aiServiceResponse = await _openRouterService.SendChatAsync<TravelPlanAIResponse>(
                systemMessage: systemMessage,
                userMessage: userMessage,
                responseFormat: responseFormat
            );

            generatedPlans = aiServiceResponse?.Items;

            if (generatedPlans == null || generatedPlans.Count != 3) 
            {
                 _logger.LogWarning("AI service returned an unexpected number of plans ({Count}) or null for user {UserId}", generatedPlans?.Count ?? 0, userId);
                 throw new AiServiceException("AI service did not return exactly 3 valid plan proposals.");
            }
        }
        catch (OpenRouterException ex)
        {
            _logger.LogError(ex, "OpenRouterService error for user {UserId}: {ErrorMessage}", userId, ex.Message);
            throw new AiServiceException($"Failed to generate plans due to an AI service error: {ex.Message}", ex);
        }
        catch (Exception ex) 
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
                Title = generatedPlan.Title!,
                Content = generatedPlan.Content!,
                Status = PlanStatus.Generated.ToString(), // Assuming PlanStatus enum or constant exists
                CreatedAt = now,
                ModifiedAt = now
            };
            newPlans.Add(plan);
        }

        // --- Step 5h: Save Plan entities --- 
        try
        {
            await context.Plans.AddRangeAsync(newPlans);
            await context.SaveChangesAsync();
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

    public async Task AcceptPlanProposalAsync(string userId, Guid planId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var plan = await context.Plans.FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

        if (plan == null)
            throw new KeyNotFoundException($"Plan with ID {planId} not found for user {userId}.");
        
        if (plan.Status != PlanStatus.Generated.ToString())
             throw new InvalidOperationException("Only plans with status 'Generated' can be accepted.");
        
        plan.Status = PlanStatus.Accepted.ToString();
        plan.ModifiedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        _logger.LogInformation("Plan proposal {PlanId} accepted by user {UserId}.", planId, userId);
    }

    public async Task RejectPlanProposalAsync(string userId, Guid planId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var plan = await context.Plans.FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

        if (plan == null)
            throw new KeyNotFoundException($"Plan with ID {planId} not found for user {userId}.");
        
        if (plan.Status != PlanStatus.Generated.ToString())
            throw new InvalidOperationException("Only plans with status 'Generated' can be rejected.");
            
        plan.Status = PlanStatus.Rejected.ToString();
        plan.ModifiedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        _logger.LogInformation("Plan proposal {PlanId} rejected by user {UserId}.", planId, userId);
    }
} 