using Domain.Contracts;

namespace Application.Helpers.Runtime;

public static class PaginationHelpers
{
    public static int GetPaginatedOffset(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            return 0;

        return (pageNumber - 1) * pageSize;
    }

    public static void UpdatePaginationProperties<T>(this PaginatedDbEntity<T> entity, int pageNumber, int pageSize)
    {
        if (entity.TotalCount <= 0)
        {
            entity.CurrentPage = 0;
            entity.PageSize = 0;
            return;
        }

        entity.CurrentPage = pageNumber;
        entity.PageSize = pageSize;
        entity.StartPage = 1;
        entity.EndPage = (entity.TotalCount / pageSize) + 1;
    }

    public static List<int> GetPageSizes(bool large = false)
    {
        if (large)
        {
            return
            [
                25,
                50,
                100,
                200,
                500
            ];
        }

        return
        [
            10,
            20,
            30,
            50
        ];
    }
}