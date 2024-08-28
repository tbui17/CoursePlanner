using System.Globalization;
using Lib.Models;

namespace ViewModels.Converters;

public class TypeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            Assessment x => $"{x.Type} Assessment",
            Course => "Course",
            _ => value?.GetType().Name ?? "No type"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}