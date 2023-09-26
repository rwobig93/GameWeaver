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
}