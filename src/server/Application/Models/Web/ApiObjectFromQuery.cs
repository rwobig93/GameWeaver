using System.Text.Json;

namespace Application.Models.Web;

public class ApiObjectFromQuery<T> where T : new()
{
    public static bool TryParse(string? value, IFormatProvider? provider, out T request)
    {
        try
        {
            var requestConverted = JsonSerializer.Deserialize<T>(value!);
            if (requestConverted is not null)
            {
                request = requestConverted;
                return true;
            }
        }
        catch
        {
            // ignored
        }

        request = new T();
        return false;
    }
}