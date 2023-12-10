using Application.Services;
using Application.Settings;
using Domain.Contracts;

namespace Infrastructure.Services;

public class HostService : IHostService
{
    public async Task<IResult> SaveSettings(IAppSettingsSection settingsSection)
    {
        // Load the file
        var filePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var json = File.ReadAllText(filePath);
        // dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        //
        // // Set the new value
        // Newtonsoft.Json.Linq.JContainer section = jsonObj[sectionPath] as Newtonsoft.Json.Linq.JContainer;
        // if (section != null)
        // {
        //     foreach (var prop in value.GetType().GetProperties())
        //     {
        //         if (prop.Name != nameof(AuthConfiguration.SectionName)) // Skip SectionName
        //         {
        //             section[prop.Name] = new Newtonsoft.Json.Linq.JValue(prop.GetValue(value));
        //         }
        //     }
        // }
        //
        // // Save the file
        // string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        // File.WriteAllText(filePath, output);

        return await Result.SuccessAsync();
    }
}