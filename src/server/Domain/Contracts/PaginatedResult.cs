﻿namespace Domain.Contracts;

public class PaginatedResult<T> : Result
{
    public PaginatedResult(List<T> data)
    {
        Data = data;
    }

    public List<T> Data { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public int TotalCount { get; set; }
    public int PageSize { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;
    // TODO: Implement 'Previous' property that returns the fulL URL w/ the query parameters needed to get the previous results, return null if there are no more results

    public bool HasNextPage => CurrentPage < TotalPages;
    // TODO: Implement 'Next' property that returns the fulL URL w/ the query parameters needed to get the next results, return null if there are no more results

    internal PaginatedResult(bool succeeded, List<T> data = default!, List<string> messages = null!, int count = 0, int page = 1, int pageSize = 10)
    {
        Data = data;
        CurrentPage = page;
        Succeeded = succeeded;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalCount = count;
        Messages = messages;
    }

    public static PaginatedResult<T> Failure(List<string> messages)
    {
        return new PaginatedResult<T>(false, default!, messages);
    }

    public static PaginatedResult<T> Success(List<T> data, int count, int page, int pageSize)
    {
        return new PaginatedResult<T>(true, data, null!, count, page, pageSize);
    }
}