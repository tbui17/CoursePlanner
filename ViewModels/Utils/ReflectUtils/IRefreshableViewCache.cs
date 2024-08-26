namespace ViewModels.Utils.ReflectUtils;

public interface IRefreshableViewCache
{
    ICollection<Type>? Get(Type target);
    void Add(Type target, ICollection<Type> views);
}