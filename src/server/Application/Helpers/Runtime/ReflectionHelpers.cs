using System.Reflection;
using Application.Database;

namespace Application.Helpers.Runtime;

public static class ReflectionHelpers
{
    public static IEnumerable<ISqlDatabaseScript> GetDbScriptsFromClass(this Type type)
    {
        var fields = type
            .GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        return (from fi in fields select fi.GetValue(null)
            into propertyValue
            where propertyValue is not null
            select (ISqlDatabaseScript)propertyValue).ToList();
    }

    public static IEnumerable<T> GetImplementingTypes<T>(this Type type)
        => AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())
            .Where(t => t.GetInterfaces().Contains(type))
            .Select(t => (T) Activator.CreateInstance(t)!)
            .ToList();

    public static List<string> GetConstantsRecursively(Type type)
    {
        // Get all public static fields of the type
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f is {IsLiteral: true, IsInitOnly: false});

        // Add the constant values to the list
        var constants = fields.Select(field => field.GetValue(null))
            .OfType<object>()
            .Select(value => value.ToString()!)
            .ToList();

        // Recursively process nested types
        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public))
        {
            constants.AddRange(GetConstantsRecursively(nestedType));
        }

        return constants;
    }

    public static long GetVersionTicks()
    {
        return new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTimeUtc.Ticks;
    }
}