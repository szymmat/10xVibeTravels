namespace _10xVibeTravels.Requests;

public class GetNotesListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "modifiedAt";
    public string SortDirection { get; set; } = "desc";
} 