namespace Application.Services.System;

public interface ISerializerService
{
    string SerializeJson<T>(T rawObject);
    T DeserializeJson<T>(string rawJson);
    T DeserializeJson<T>(byte[] rawJson);
    byte[] SerializeMemory<T>(T rawObject);
    T? DeserializeMemory<T>(byte[] rawMemory);
}