namespace _10xVibeTravels.Dtos;

// Note: This could potentially be moved to a more common location like Application/Common/DTOs
// ^^^ Keeping original note for context, but it's now in the root Dtos folder.
public record PaginatedListDto<T>
(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
); 