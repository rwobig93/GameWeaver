using System.Text.Json;

namespace Application.Helpers.Runtime;

public static class JsonHelpers
{
    public static JsonElement? GetNestedValue(this JsonElement element, string path)
    {
        var parts = path.Split('.');
        foreach (var part in parts)
        {
            if (element.TryGetProperty(part, out var tempElement))
            {
                element = tempElement;
            }
            else
            {
                return null;
            }
        }
        return element;
    }
}