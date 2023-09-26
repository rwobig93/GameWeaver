namespace Application.Constants.Lifecycle;

public static class AuditTrailConstants
{
    public static readonly List<string> DiffPropertiesToIgnore = new()
    {
        "PasswordHash",
        "PasswordSalt",
        "CreatedBy",
        "CreatedOn",
        "LastModifiedBy",
        "LastModifiedOn"
    };
}