namespace _10xVibeTravels.Dtos;

public record NoteListItemDto
(
    Guid Id,
    string Title,
    string ContentPreview,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt
); 