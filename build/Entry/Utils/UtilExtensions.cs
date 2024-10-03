using System.Runtime.CompilerServices;

namespace Entry.Utils;

public static class UtilExtensions
{
    public static void Log<T>(this T obj, [CallerMemberName] string caller = "")
    {
        var logger = Serilog.Log.ForContext<Build>();
        logger.Information("{Caller}: {@Object}", caller, obj);
    }
}