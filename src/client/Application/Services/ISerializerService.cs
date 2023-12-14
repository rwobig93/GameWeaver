using Domain.Contracts;

namespace Application.Services;

public interface ISerializerService
{
    string Serialize<T>(T rawObject);
    T Deserialize<T>(string rawJson);
    T Deserialize<T>(byte[] rawJson);
    Task<IResult> SaveSettings(string settingsSectionName, object updatedSection);
}