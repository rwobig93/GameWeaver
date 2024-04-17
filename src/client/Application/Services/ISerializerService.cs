namespace Application.Services;

public interface ISerializerService
{
    string SerializeJson<T>(T rawObject);
    T DeserializeJson<T>(string rawJson);
    T DeserializeJson<T>(byte[] rawJson);
    Task<IResult> SaveSettings(string settingsSectionName, object updatedSection);
    byte[] SerializeMemory<T>(T rawObject);
    T? DeserializeMemory<T>(byte[] rawMemory);
}