namespace Application.Constants.Lifecycle;

public static class AuditTrailConstants
{
    public static readonly List<string> DiffPropertiesToIgnore =
    [
        "PasswordHash",
        "PasswordSalt",
        "CreatedBy",
        "CreatedOn",
        "LastModifiedBy",
        "LastModifiedOn"
    ];
}