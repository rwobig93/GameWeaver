using Application.Constants;
using Application.Services;
using Application.Settings;
using Domain.Contracts;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Services;

public class HostService : IHostService
{
    public async Task<IResult> SaveSettings(string settingsSectionName, object updatedSection)
    {
        // Load the file
        var filePath = Path.Combine(AppContext.BaseDirectory, HostConstants.ConfigFile);
        var json = File.ReadAllText(filePath);
        dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json)!;
        
        // Set the new value
        JContainer section = jsonObj[settingsSectionName];
        if (section is not null)
        {
            foreach (var prop in updatedSection.GetType().GetProperties())
            {
                if (prop.Name != nameof(settingsSectionName)) // Skip SectionName
                {
                    section[prop.Name] = new JValue(prop.GetValue(updatedSection));
                }
            }
        }
        
        // Save the file
        string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(filePath, output);

        return await Result.SuccessAsync();
    }
}