using _10xVibeTravels.Dtos;
using _10xVibeTravels.Requests;

namespace _10xVibeTravels.Interfaces;

public interface INoteService
{
    Task<PaginatedListDto<NoteListItemDto>> GetNotesAsync(string userId, GetNotesListQuery query);

    Task<NoteDto> CreateNoteAsync(string userId, CreateNoteRequest request);

    Task<NoteDto?> GetNoteByIdAsync(string userId, Guid noteId);

    Task<NoteDto?> UpdateNoteAsync(string userId, Guid noteId, UpdateNoteRequest request);

    Task<bool> DeleteNoteAsync(string userId, Guid noteId);
} 