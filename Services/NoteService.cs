using System.Security.Claims;
using _10xVibeTravels.Data;
using _10xVibeTravels.Dtos;
using _10xVibeTravels.Exceptions;
using _10xVibeTravels.Interfaces;
using _10xVibeTravels.Requests;
using Microsoft.EntityFrameworkCore;

namespace _10xVibeTravels.Services;

public class NoteService : INoteService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NoteService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
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
        var note = new Note
        {
            Id = Guid.NewGuid(), // Generate new ID
            UserId = userId,      // Assign current user
            Title = request.Title,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow, // Use DateTime
            ModifiedAt = DateTime.UtcNow // Use DateTime
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

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
        var note = await _context.Notes
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

        _context.Notes.Remove(note);
        var result = await _context.SaveChangesAsync();

        return result > 0; // Return true if deletion was successful
    }

    public async Task<NoteDto?> GetNoteByIdAsync(string userId, Guid noteId)
    {
        var note = await _context.Notes
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

    public async Task<PaginatedListDto<NoteListItemDto>> GetNotesAsync(string userId, GetNotesListQuery query)
    {
        // Start with base query filtered by user
        var queryable = _context.Notes
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        // Apply sorting
        bool descending = query.SortDirection?.ToLowerInvariant() == "desc";
        queryable = query.SortBy?.ToLowerInvariant() switch
        {
            "createdat" => descending 
                ? queryable.OrderByDescending(n => n.CreatedAt) 
                : queryable.OrderBy(n => n.CreatedAt),
            _ => descending // Default to ModifiedAt descending
                ? queryable.OrderByDescending(n => n.ModifiedAt) 
                : queryable.OrderBy(n => n.ModifiedAt),
        };

        // Calculate total items before pagination
        var totalItems = await queryable.CountAsync();

        // Apply pagination
        var notes = await queryable
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(); // Execute query to get the page items

        // Map to DTO, creating ContentPreview
        var items = notes.Select(n => new NoteListItemDto(
            n.Id,
            n.Title,
            n.Content.Length > 150 ? n.Content.Substring(0, 150) + "..." : n.Content, // Generate preview
            n.CreatedAt,
            n.ModifiedAt
        )).ToList(); // Use ToList() here for IReadOnlyCollection compatibility

        var totalPages = (int)Math.Ceiling((double)totalItems / query.PageSize);

        return new PaginatedListDto<NoteListItemDto>(
            items,
            query.Page,
            query.PageSize,
            totalItems,
            totalPages
        );
    }

    public async Task<NoteDto?> UpdateNoteAsync(string userId, Guid noteId, UpdateNoteRequest request)
    {
        var note = await _context.Notes
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

        await _context.SaveChangesAsync();

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