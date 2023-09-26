namespace Application.Helpers.Runtime;

public static class MathHelpers
{
    public static int GetPaginatedOffset(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            return 0;
        
        return (pageNumber - 1) * pageSize;
    }
}