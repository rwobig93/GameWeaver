using Application.Models.Identity.Permission;

namespace Application.Helpers.Identity;

public class PermissionCreateComparer : IEqualityComparer<AppPermissionCreate>
{
    public bool Equals(AppPermissionCreate? x, AppPermissionCreate? y)
    {
        if (x is null || y is null)
            return false;

        return x.Group == y.Group &&
               x.Name == y.Name &&
               x.Access == y.Access;
    }

    public int GetHashCode(AppPermissionCreate create)
    {
        return $"{create.Group}{create.Name}{create.Access}".GetHashCode();
    }
}