using System.Reflection;

namespace BuildLib.Utils;

public class ObjectNode
{
    public ObjectNode? Parent;
    public PropertyInfo? Property;
    public object? Value;

    public bool CanRecurse => Value is not null and not string
                              && Property?.PropertyType is { IsClass: true, IsPrimitive: false };


    public IEnumerable<ObjectNode> WalkToRoot()
    {
        var current = this;
        while (current != null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public IEnumerable<string> GetPath()
    {
        return WalkToRoot()
            .Reverse()
            .GroupBy(x => x.Property is null)
            .Select(group => group.Key switch
                {
                    true => group.Single().Value!.GetType().Name,
                    false => group.Select(x => x.Property!.Name).Thru(x => string.Join(".", x))
                }
            );
    }
}