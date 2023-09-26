namespace Application.Database;

public interface ISqlEnforcedEntity
{
    // Any class inheriting this interface will be enforced in the targeted database instance
    // The below method also needs to be the following w/ the correct class name on any inheriting class:
    //
    // public IEnumerable<ISqlDatabaseScript> GetDbScripts() => typeof(ClassName).GetDbScriptsFromClass();
    //
    // Database script enforcement order/priority is determined by the EnforcementOrder property on classes inheriting ISqlDatabaseScript
    // For more details please see ISqlDatabaseScript.cs
    public IEnumerable<ISqlDatabaseScript> GetDbScripts();
}