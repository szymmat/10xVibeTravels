using _10xVibeTravels.Data;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Requests;
using _10xVibeTravels.Responses;
using _10xVibeTravels.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace _10xVibeTravels.Services;

public class PlanService : IPlanService
{
    private readonly ApplicationDbContext _context;
    // private readonly IMapper _mapper; // TODO: Add AutoMapper later if needed

    public PlanService(ApplicationDbContext context/*, IMapper mapper*/)
    {
        _context = context;
        // _mapper = mapper; 
    }

    public async Task<PaginatedResult<PlanListItemDto>> GetPlansAsync(string userId, PlanListQueryParameters queryParams)
    {
        var query = _context.Plans
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.Status == queryParams.Status); // Base filter

        // Sorting
        var sortColumn = GetSortProperty(queryParams.SortBy);
        query = queryParams.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(sortColumn)
            : query.OrderByDescending(sortColumn);

        // Pagination
        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(p => new PlanListItemDto // Use non-qualified DTO name
            {
                Id = p.Id,
                Status = Enum.Parse<PlanStatus>(p.Status, true), // Map string to enum
                Title = p.Title,
                ContentPreview = p.Content != null && p.Content.Length > 150 ? p.Content.Substring(0, 150) + "..." : p.Content ?? string.Empty, // Handle null content
                StartDate = DateOnly.FromDateTime(p.StartDate), // Convert DateTime to DateOnly
                EndDate = DateOnly.FromDateTime(p.EndDate),     // Convert DateTime to DateOnly
                Budget = p.Budget,
                CreatedAt = p.CreatedAt,
                ModifiedAt = p.ModifiedAt
            })
            .ToListAsync();

        return new PaginatedResult<PlanListItemDto>(items, totalItems, queryParams.Page, queryParams.PageSize);
    }

    public async Task<PlanDetailDto?> GetPlanByIdAsync(string userId, Guid planId)
    {
        var plan = await _context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

        if (plan == null)
        {
            return null;
        }

        // Manual Projection
        return new PlanDetailDto // Use non-qualified DTO name
        {
            Id = plan.Id,
            Status = Enum.Parse<PlanStatus>(plan.Status, true),
            Title = plan.Title,
            Content = plan.Content ?? string.Empty, // Handle null content
            StartDate = DateOnly.FromDateTime(plan.StartDate), // Convert DateTime to DateOnly
            EndDate = DateOnly.FromDateTime(plan.EndDate),     // Convert DateTime to DateOnly
            Budget = plan.Budget,
            CreatedAt = plan.CreatedAt,
            ModifiedAt = plan.ModifiedAt
        };
    }

    public async Task<(PlanDetailDto? UpdatedPlan, string? ErrorMessage)> UpdatePlanAsync(string userId, Guid planId, UpdatePlanDto updateDto)
    {
        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

        if (plan == null)
        {
            return (null, "Plan not found or you don't have access.");
        }

        var currentStatus = Enum.Parse<PlanStatus>(plan.Status, true);

        // Business logic validation
        if (currentStatus == PlanStatus.Generated)
        {
            if (updateDto.Content != null)
            {
                return (null, "Cannot update content for a 'Generated' plan.");
            }
            if (updateDto.Status.HasValue && !(updateDto.Status == PlanStatus.Accepted || updateDto.Status == PlanStatus.Rejected))
            {
                return (null, "Invalid status transition for 'Generated' plan. Only 'Accepted' or 'Rejected' allowed.");
            }

            // Apply allowed changes for Generated status
            if (updateDto.Status.HasValue)
            {
                plan.Status = updateDto.Status.Value.ToString();
            }
            if (updateDto.Title != null)
            {
                plan.Title = updateDto.Title;
            }
        }
        else if (currentStatus == PlanStatus.Accepted)
        {
            if (updateDto.Status.HasValue || updateDto.Title != null)
            {
                return (null, "Cannot update status or title for an 'Accepted' plan.");
            }
            // Apply allowed changes for Accepted status
            if (updateDto.Content != null)
            {
                plan.Content = updateDto.Content;
            }
        }
        else // Rejected or other potential future statuses
        {
            return (null, $"Cannot update a plan with status '{plan.Status}'.");
        }
        
        // If any change was made, update ModifiedAt
        if (_context.Entry(plan).State == EntityState.Modified)
        {
             plan.ModifiedAt = DateTime.UtcNow;
        }
        else 
        {
             // No changes detected, return the current state as DTO 
        }

        await _context.SaveChangesAsync();

        // Project updated entity to DTO
        var updatedDto = new PlanDetailDto // Use non-qualified DTO name
        {
           Id = plan.Id,
            Status = Enum.Parse<PlanStatus>(plan.Status, true),
            Title = plan.Title,
            Content = plan.Content ?? string.Empty, // Handle null content
            StartDate = DateOnly.FromDateTime(plan.StartDate), // Convert DateTime to DateOnly
            EndDate = DateOnly.FromDateTime(plan.EndDate),     // Convert DateTime to DateOnly
            Budget = plan.Budget,
            CreatedAt = plan.CreatedAt,
            ModifiedAt = plan.ModifiedAt
        };

        return (updatedDto, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeletePlanAsync(string userId, Guid planId)
    {
        var plan = await _context.Plans.FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

        if (plan == null)
        {
            // Return true for idempotency (resource is already gone)
            return (true, null); 
        }

        _context.Plans.Remove(plan);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    // Helper to get Expression for sorting (avoids magic strings directly in OrderBy)
    private static Expression<Func<Plan, object>> GetSortProperty(string? sortBy)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "createdat" => plan => plan.CreatedAt,
            "modifiedat" => plan => plan.ModifiedAt, // Default
            _ => plan => plan.ModifiedAt // Default if null or invalid
        };
    }
} 