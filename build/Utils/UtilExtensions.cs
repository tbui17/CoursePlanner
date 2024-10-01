using System;

namespace Utils;

static class UtilExtensions
{
    public static T2 Thru<T1, T2>(this T1 t1, Func<T1, T2> func) => func(t1);
    public static void Thru<T1>(this T1 t1, Action<T1> func) => func(t1);
}