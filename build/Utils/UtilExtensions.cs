using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utils;

static class UtilExtensions
{
    public static T2 Thru<T1, T2>(this T1 t1, Func<T1, T2> func) => func(t1);
    public static void Thru<T1>(this T1 t1, Action<T1> func) => func(t1);

    public static Dictionary<string, object> ToPropertyDictionary<T>(this T obj) where T : class
    {
        return obj
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(x => x.CanRead)
            .ToDictionary(x => x.Name, x => x.GetValue(obj));
    }
}