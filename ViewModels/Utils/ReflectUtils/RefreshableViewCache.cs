namespace ViewModels.Utils.ReflectUtils;

public class RefreshableViewCache : IRefreshableViewCache
{
    private Dictionary<Type, ICollection<Type>> Cache { get; } = new();

    public ICollection<Type>? Get(Type target)
    {
        return Cache.TryGetValue(target, out var cached) ? cached : null;
    }

    public void Add(Type target, ICollection<Type> views)
    {
        Cache[target] = views;
    }
}