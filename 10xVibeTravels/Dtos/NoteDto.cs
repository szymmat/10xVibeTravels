namespace _10xVibeTravels.Dtos;

public record NoteDto
(
    Guid Id,
    string Title,
    string Content,
    DateTimeOffset CreatedAt,
    DateTimeOffset ModifiedAt
); 