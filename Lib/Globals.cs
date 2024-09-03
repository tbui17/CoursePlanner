namespace Lib;

public static class Globals
{
    public static DateTime DefaultStart() => DateTime.Now;
    public static DateTime DefaultEnd() => DateTime.Now.AddDays(1);
}