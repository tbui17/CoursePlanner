using System.Globalization;
using Lib.Interfaces;

namespace ViewModels.Converters;

public class TypeToStringConverter : IValueConverter
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        return value switch
        {
            IFriendlyType x => x.GetFriendlyType(),
            _ => value?.GetType().Name ?? "No type"
        };
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}