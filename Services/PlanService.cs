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
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    // private readonly IMapper _mapper; // TODO: Add AutoMapper later if needed

    public PlanService(IDbContextFactory<ApplicationDbContext> contextFactory/*, IMapper mapper*/)
    {
        _contextFactory = contextFactory;
        // _mapper = mapper; 
    }

    public async Task<PaginatedResult<PlanListItemDto>> GetPlansAsync(string userId, PlanListQueryParameters queryParams)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var queryable = context.Plans
            .AsNoTracking()
            .Where(p => p.UserId == userId); 

        if (!string.IsNullOrEmpty(queryParams.Status) && Enum.TryParse<PlanStatus>(queryParams.Status, true, out var statusEnum))
        {
            string statusString = statusEnum.ToString(); 
            queryable = queryable.Where(p => p.Status == statusString);
        }

        Expression<Func<Plan, object>> keySelector = GetSortPropertyExpression(queryParams.SortBy);
        queryable = queryParams.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
            ? queryable.OrderBy(keySelector)
            : queryable.OrderByDescending(keySelector);

        var totalItems = await queryable.CountAsync();
        var items = await queryable
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(p => new PlanListItemDto 
            {
                Id = p.Id,
                Status = Enum.Parse<PlanStatus>(p.Status, true),
                Title = p.Title,
                ContentPreview = p.Content != null && p.Content.Length > 150 ? p.Content.Substring(0, 150) + "..." : p.Content ?? string.Empty,
                StartDate = DateOnly.FromDateTime(p.StartDate),
                EndDate = DateOnly.FromDateTime(p.EndDate),
                Budget = p.Budget,
                CreatedAt = p.CreatedAt,
                ModifiedAt = p.ModifiedAt
            })
            .ToListAsync();

        return new PaginatedResult<PlanListItemDto>(items, totalItems, queryParams.Page, queryParams.PageSize);
    }

    public async Task<PlanDetailDto?> GetPlanByIdAsync(string userId, Guid planId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var plan = await context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

        if (plan == null)
        {
            return null;
        }

        return new PlanDetailDto 
        {
            Id = plan.Id,
            Status = Enum.Parse<PlanStatus>(plan.Status, true),
            Title = plan.Title,
            Content = plan.Content ?? string.Empty,
            StartDate = DateOnly.FromDateTime(plan.StartDate),
            EndDate = DateOnly.FromDateTime(plan.EndDate),
            Budget = plan.Budget,
            CreatedAt = plan.CreatedAt,
            ModifiedAt = plan.ModifiedAt
        };
    }

    public async Task<(PlanDetailDto? UpdatedPlan, string? ErrorMessage)> UpdatePlanAsync(string userId, Guid planId, UpdatePlanDto updateDto)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        
        var plan = await context.Plans.FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

        if (plan == null)
        {
            return (null, "Plan not found or you don't have access.");
        }

        if (!Enum.TryParse<PlanStatus>(plan.Status, true, out var currentStatus))
        {
            return (null, $"Invalid current status '{plan.Status}' found in database.");
        }

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
            if (updateDto.Content != null)
            {
                plan.Content = updateDto.Content;
            }
        }
        else 
        {
            return (null, $"Cannot update a plan with status '{currentStatus}'.");
        }
        
        bool hasChanges = context.Entry(plan).State == EntityState.Modified;
        if (hasChanges)
        {
             plan.ModifiedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();

        var updatedDto = new PlanDetailDto 
        {
           Id = plan.Id,
            Status = Enum.Parse<PlanStatus>(plan.Status, true),
            Title = plan.Title,
            Content = plan.Content ?? string.Empty,
            StartDate = DateOnly.FromDateTime(plan.StartDate),
            EndDate = DateOnly.FromDateTime(plan.EndDate),
            Budget = plan.Budget,
            CreatedAt = plan.CreatedAt,
            ModifiedAt = plan.ModifiedAt
        };

        return (updatedDto, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeletePlanAsync(string userId, Guid planId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var plan = await context.Plans.FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId);

        if (plan == null)
        {
            return (true, null); 
        }

        context.Plans.Remove(plan);
        await context.SaveChangesAsync();

        return (true, null);
    }

    private static Expression<Func<Plan, object>> GetSortPropertyExpression(string? sortBy)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "title" => plan => plan.Title,
            "startdate" => plan => plan.StartDate,
            "createdat" => plan => plan.CreatedAt,
            _ => plan => plan.ModifiedAt
        };
    }
} 