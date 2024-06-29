using System.Globalization;
using Lib.Models;

namespace CoursePlanner.Converters;

public class CourseListToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not ICollection<Course> courses)
        {
            throw new ArgumentException("Unexpected value type.");
        }

        return string.Join(", ", courses.Select(x => x.Name));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}