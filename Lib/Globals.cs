namespace Lib;

public static class Globals
{
    public static DateTime DefaultStart() => DateTime.Now.Date;
    public static DateTime DefaultEnd() => DefaultStart().AddDays(1);
}