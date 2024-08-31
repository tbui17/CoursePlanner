namespace Lib.Utils;

public static class DateExtensions
{
    public static bool DatesEqual(DateTime a, DateTime b)
    {
        return a.Year == b.Year && a.Month == b.Month && a.Day == b.Day;
    }

}