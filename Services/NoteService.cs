using System.Security.Claims;
using _10xVibeTravels.Data;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Exceptions;
using _10xVibeTravels.Interfaces;
using _10xVibeTravels.Requests;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace _10xVibeTravels.Services;

public class NoteService : INoteService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NoteService(IDbContextFactory<ApplicationDbContext> contextFactory, IHttpContextAccessor httpContextAccessor)
    {
        _contextFactory = contextFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            // This should ideally not happen if [Authorize] is used correctly, 
            // but adding a fallback exception for robustness.
            throw new UnauthorizedAccessException("User ID could not be retrieved from the context.");
        }
        return userId;
    }

    public async Task<NoteDto> CreateNoteAsync(string userId, CreateNoteRequest request)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var note = new Note
        {
            Id = Guid.NewGuid(), // Generate new ID
            UserId = userId,      // Assign current user
            Title = request.Title,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow, // Use DateTime
            ModifiedAt = DateTime.UtcNow // Use DateTime
        };

        context.Notes.Add(note);
        await context.SaveChangesAsync();

        // Map entity to DTO for the response
        return new NoteDto(
            note.Id,
            note.Title,
            note.Content,
            note.CreatedAt, // Pass DateTime to DTO
            note.ModifiedAt  // Pass DateTime to DTO
        );
    }

    public async Task<bool> DeleteNoteAsync(string userId, Guid noteId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var note = await context.Notes
            .FirstOrDefaultAsync(n => n.Id == noteId);

        if (note == null)
        {
            // Note: Depending on API design philosophy, one might choose 
            // to return false or even 204 No Content here instead of 404.
            // The plan specifies 404 for Not Found, so we throw.
            throw new NoteNotFoundException(noteId);
        }

        if (note.UserId != userId)
        {
            // Similar to above, could return false/403, but plan implies throwing.
            throw new NoteAccessDeniedException(noteId, userId);
        }

        context.Notes.Remove(note);
        var result = await context.SaveChangesAsync();

        return result > 0; // Return true if deletion was successful
    }

    public async Task<NoteDto?> GetNoteByIdAsync(string userId, Guid noteId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var note = await context.Notes
            .AsNoTracking() // Read-only operation
            .FirstOrDefaultAsync(n => n.Id == noteId);

        if (note == null)
        {
            throw new NoteNotFoundException(noteId); // Revert to original, logically correct call
        }

        if (note.UserId != userId)
        {
            // Important: Check ownership AFTER confirming existence 
            // to avoid leaking information about note IDs.
            throw new NoteAccessDeniedException(noteId, userId);
        }

        // Map entity to DTO
        return new NoteDto(
            note.Id,
            note.Title,
            note.Content,
            note.CreatedAt,
            note.ModifiedAt
        );
    }

    public async Task<PaginatedListDto<NoteListItemDto>> GetNotesAsync(string userId, GetNotesListQuery queryParams)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var queryable = context.Notes
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        bool descending = queryParams.SortDirection?.ToLowerInvariant() == "desc";
        Expression<Func<Note, object>> keySelector = queryParams.SortBy?.ToLowerInvariant() switch
        {
            "title" => note => note.Title,
            "createdat" => note => note.CreatedAt,
            _ => note => note.ModifiedAt
        };

        if (descending)
        {
            queryable = queryable.OrderByDescending(keySelector);
        }
        else
        {
            queryable = queryable.OrderBy(keySelector);
        }

        var totalItems = await queryable.CountAsync();

        var notes = await queryable
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var items = notes.Select(n => new NoteListItemDto(
            n.Id,
            n.Title,
            n.Content.Length > 150 ? n.Content.Substring(0, 150) + "..." : n.Content,
            n.CreatedAt,
            n.ModifiedAt
        )).ToList();

        var totalPages = (int)Math.Ceiling((double)totalItems / queryParams.PageSize);

        return new PaginatedListDto<NoteListItemDto>(
            items,
            queryParams.Page,
            queryParams.PageSize,
            totalItems,
            totalPages
        );
    }

    public async Task<NoteDto?> UpdateNoteAsync(string userId, Guid noteId, UpdateNoteRequest request)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var note = await context.Notes
            .FirstOrDefaultAsync(n => n.Id == noteId);

        if (note == null)
        {
            throw new NoteNotFoundException(noteId);
        }

        if (note.UserId != userId)
        {
            throw new NoteAccessDeniedException(noteId, userId);
        }

        // Update properties
        note.Title = request.Title;
        note.Content = request.Content;
        note.ModifiedAt = DateTime.UtcNow; // Update modification timestamp

        await context.SaveChangesAsync();

        // Map updated entity to DTO
        return new NoteDto(
            note.Id,
            note.Title,
            note.Content,
            note.CreatedAt, // Keep original creation date
            note.ModifiedAt
        );
    }
} 