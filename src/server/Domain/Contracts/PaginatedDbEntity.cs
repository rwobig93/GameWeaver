namespace Domain.Contracts;

public class PaginatedDbEntity<T>
{
    public T Data { get; set; } = default!;
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public int StartPage { get; set; }
    public int CurrentPage { get; set; }
    public int EndPage { get; set; }
}