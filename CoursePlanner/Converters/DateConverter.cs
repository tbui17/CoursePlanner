using System.Globalization;

namespace CoursePlanner.Converters;

public class DateConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime date)
        {
            throw new ArgumentException("Unexpected value type.");
        }

        return date.ToShortDateString();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return DateTime.Parse(value?.ToString() ?? string.Empty);
    }
}