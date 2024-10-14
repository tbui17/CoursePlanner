using Serilog;

namespace BuildTests.Utils;

public static class UtilExtensions
{
    public static void Dump<T>(this T obj)
    {
        Log.Debug("{@obj}", obj);
    }
}