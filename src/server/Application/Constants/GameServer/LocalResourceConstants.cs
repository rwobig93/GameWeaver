using Domain.Enums.GameServer;

namespace Application.Constants.GameServer;

public static class LocalResourceConstants
{
    public static readonly List<string> ImportConfigExtensions = [".ini", ".json", ".xml", ".cfg", ".conf"];
    public static readonly List<string> ImportScriptExtensions = [".bat", ".batch", ".cmd", ".sh", ".ps1", ".py", ".vb", ".lua"];
    public static readonly ContentType[] AddResourceValidConfigTypes = [ContentType.Ini, ContentType.Json, ContentType.Xml, ContentType.Raw];
}