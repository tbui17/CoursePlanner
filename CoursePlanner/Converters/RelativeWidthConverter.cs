using System.Globalization;

namespace CoursePlanner.Converters;

public class RelativeWidthConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int or double or decimal or long)
        {
            var val = (double)value;
            return val * 0.8;

        }

        throw new ArgumentException("Unexpected value type.");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}