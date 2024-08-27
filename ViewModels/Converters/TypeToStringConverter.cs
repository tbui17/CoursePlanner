using System.Globalization;
using Lib.Models;

namespace ViewModels.Converters;

public class TypeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Assessment a)
        {
            return $"{a.Type} Assessment";
        }
        return value?.GetType().Name ?? "No type";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}