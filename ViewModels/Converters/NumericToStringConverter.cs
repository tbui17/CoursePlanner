using System.Globalization;

namespace ViewModels.Converters;

public class NumericToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {

        var res = value switch
        {
            double x => x.ToString(),
            float x => x.ToString(),
            int x => x.ToString(),
            long x => x.ToString(),
            decimal x => x.ToString(),
            _ => throw new ArgumentException("Invalid type")
        };

        if (string.IsNullOrWhiteSpace(res))
        {
            return "0";
        }

        return res;

    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}