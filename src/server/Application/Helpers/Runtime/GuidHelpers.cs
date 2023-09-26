namespace Application.Helpers.Runtime;

public static class GuidHelpers
{
    public static Guid GetMax()
    {
        return new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
    }

    public static Guid GetFromNullable(this Guid? guid)
    {
        return guid is null ? Guid.Empty : Guid.Parse(guid.ToString()!);
    }
}