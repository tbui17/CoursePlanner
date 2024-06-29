using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Lib.Utils;

public static class UtilExtensions
{
    public static async Task<ObservableCollection<T>> ToObservableCollectionAsync<T>(this Task<List<T>> collection)
    {
        return new ObservableCollection<T>(await collection);
    }

    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
    {
        return new ObservableCollection<T>(collection);
    }

    public static T Get<T>(this IDictionary<string, object> dictionary, string key) => (T)dictionary[key];

    public static bool TryGet<T>(this IDictionary<string, object> dictionary, string key, out T value)
    {
        if (dictionary.TryGetValue(key, out var obj))
        {
            value = (T)obj;
            return true;
        }

        value = default!;
        return false;
    }
    
    public static string StringJoin<T>(this IEnumerable<T> collection, string separator = "") => string.Join(separator, collection);
}

public static class Util
{
   
    
    public static string FullPath(string path)
    {
        var start = path.IndexOf('(') + 1;
        var end = path.IndexOf(')');
        var str = path[start..end];
        
        return str;
    }
}