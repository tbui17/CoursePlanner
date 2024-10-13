using System.Reflection;
using System.Runtime.CompilerServices;

namespace BuildLib.Utils;

public static class TypeExtensions
{
    public static ConstructorInfo? GetParameterlessConstructor(this Type type)
    {
        return type
            .GetConstructors()
            .SingleOrDefault(x => x.GetParameters().Length == 0);
    }

    public static bool HasParameterlessConstructor(this Type type)
    {
        return type.GetParameterlessConstructor() is not null;
    }

    public static IEnumerable<PropertyInfo> GetRequiredProperties(this Type type)
    {
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => x.GetCustomAttribute<RequiredMemberAttribute>() is not null ? x : null)
            .OfType<PropertyInfo>();
    }

    public static bool HasRequiredProperties(this Type type)
    {
        return type
            .GetRequiredProperties()
            .Any();
    }

    public static bool IsSimpleType(this Type type)
    {
        if (type.IsPrimitive)
        {
            return true;
        }

        if (type == typeof(string))
        {
            return true;
        }

        if (type.IsClass && type.HasParameterlessConstructor() && !type.HasRequiredProperties())
        {
            return true;
        }

        return false;
    }

    public static bool HasSimpleConstructor(this Type type)
    {
        return type
            .GetConstructors()
            .SelectMany(x => x.GetParameters())
            .Where(x => x.ParameterType.IsSimpleType())
            .Any();
    }
}