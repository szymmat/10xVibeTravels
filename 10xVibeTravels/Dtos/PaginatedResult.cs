namespace _10xVibeTravels.Dtos;

public class PaginatedResult<T>
{
    public List<T> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }

    public PaginatedResult(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        Page = pageNumber;
        PageSize = pageSize;
        TotalItems = count;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }
} 