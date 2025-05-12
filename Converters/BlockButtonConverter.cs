using System.Globalization;
using System.Windows.Data;

namespace AIImageGuide.Converters;

public class BlockButtonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "Розблокувати" : "Заблокувати";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
