namespace Application.Database;

public interface ISqlMigration
{
    public Version VersionTarget { get; }
    public string Up { get; }
    public string Down { get; }
}