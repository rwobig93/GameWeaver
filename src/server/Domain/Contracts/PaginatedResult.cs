namespace Domain.Contracts;

public class PaginatedResult<T> : Result<T>
{
    public int StartPage { get; set; }
    public int CurrentPage { get; set; }
    public int EndPage { get; set; }
    public int TotalCount { get; set; }
    public int PageSize { get; set; }
    public string? Previous { get; set; }
    public string? Next { get; set; }

    public new static PaginatedResult<T> Fail()
    {
        return new PaginatedResult<T> { Succeeded = false };
    }

    public new static PaginatedResult<T> Fail(string message)
    {
        return new PaginatedResult<T> { Succeeded = false, Messages = [message]};
    }

    public new static PaginatedResult<T> Fail(List<string> messages)
    {
        return new PaginatedResult<T> { Succeeded = false, Messages = messages };
    }

    public static PaginatedResult<T> Fail(T data, int currentPage = 0, int totalCount = 0, int pageSize = 0, string? previous = null, string? next = null)
    {
        return new PaginatedResult<T> { Succeeded = false, Data = data, CurrentPage = currentPage, TotalCount = totalCount, PageSize = pageSize, Previous = previous, Next = next};
    }

    public static PaginatedResult<T> Fail(T data, string message, int currentPage = 0, int totalCount = 0, int pageSize = 0, string? previous = null,
        string? next = null)
    {
        return new PaginatedResult<T> { Succeeded = false, Data = data, Messages = [message], CurrentPage = currentPage, TotalCount = totalCount,
            PageSize = pageSize, Previous = previous, Next = next };
    }

    public static PaginatedResult<T> Fail(T data, List<string> messages, int currentPage = 0, int totalCount = 0, int pageSize = 0, string? previous = null,
        string? next = null)
    {
        return new PaginatedResult<T> { Succeeded = false, Data = data, Messages = messages, CurrentPage = currentPage, TotalCount = totalCount, PageSize = pageSize,
            Previous = previous, Next = next };
    }

    public new static Task<PaginatedResult<T>> FailAsync()
    {
        return Task.FromResult(Fail());
    }

    public new static Task<PaginatedResult<T>> FailAsync(string message)
    {
        return Task.FromResult(Fail(message));
    }

    public new static Task<PaginatedResult<T>> FailAsync(List<string> messages)
    {
        return Task.FromResult(Fail(messages));
    }

    public static Task<PaginatedResult<T>> FailAsync(T data, int currentPage = 0, int totalCount = 0, int pageSize = 0, string? previous = null, string? next = null)
    {
        return Task.FromResult(Fail(data, currentPage, totalCount, pageSize, previous, next));
    }

    public static Task<PaginatedResult<T>> FailAsync(T data, string message, int currentPage = 0, int totalCount = 0, int pageSize = 0, string? previous = null,
        string? next = null)
    {
        return Task.FromResult(Fail(data, message, currentPage, totalCount, pageSize, previous, next));
    }

    public static Task<PaginatedResult<T>> FailAsync(T data, List<string> messages, int currentPage = 0, int totalCount = 0, int pageSize = 0,
        string? previous = null, string? next = null)
    {
        return Task.FromResult(Fail(data, messages, currentPage, totalCount, pageSize, previous, next));
    }

    public new static PaginatedResult<T> Success()
    {
        return new PaginatedResult<T> { Succeeded = true };
    }

    public static PaginatedResult<T> Success(T data, int startPage = 0, int currentPage = 0, int endPage = 0, int totalCount = 0,
        int pageSize = 0, string? previous = null, string? next = null)
    {
        return new PaginatedResult<T>
        {
            Succeeded = true,
            Data = data,
            StartPage = startPage,
            CurrentPage = currentPage,
            EndPage = endPage,
            TotalCount = totalCount,
            PageSize = pageSize,
            Previous = previous,
            Next = next
        };
    }

    public static PaginatedResult<T> Success(T data, string message, int currentPage = 0, int totalCount = 0, int pageSize = 0, string? previous = null,
        string? next = null)
    {
        return new PaginatedResult<T> { Succeeded = true, Data = data, Messages = [message], CurrentPage = currentPage, TotalCount = totalCount,
            PageSize = pageSize, Previous = previous, Next = next };
    }

    public static PaginatedResult<T> Success(T data, List<string> messages, int currentPage = 0, int totalCount = 0, int pageSize = 0, string? previous = null,
        string? next = null)
    {
        return new PaginatedResult<T> { Succeeded = true, Data = data, Messages = messages, CurrentPage = currentPage, TotalCount = totalCount, PageSize = pageSize,
            Previous = previous, Next = next };
    }

    public new static Task<PaginatedResult<T>> SuccessAsync()
    {
        return Task.FromResult(Success());
    }

    public static Task<PaginatedResult<T>> SuccessAsync(T data, int startPage = 0, int currentPage = 0, int endPage = 0, int totalCount = 0,
        int pageSize = 0, string? previous = null, string? next = null)
    {
        return Task.FromResult(Success(data, startPage, currentPage, endPage, totalCount, pageSize, previous, next));
    }

    public static Task<PaginatedResult<T>> SuccessAsync(T data, string message, int currentPage = 0, int totalCount = 0, int pageSize = 0,
        string? previous = null, string? next = null)
    {
        return Task.FromResult(Success(data, message, currentPage, totalCount, pageSize, previous, next));
    }
}
