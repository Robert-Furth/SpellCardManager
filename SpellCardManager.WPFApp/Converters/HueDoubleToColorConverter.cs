using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SpellCardManager.WPFApp.Converters;

class HueDoubleToColorConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is not double hueF) return DependencyProperty.UnsetValue;
        return ColorExtensions.ColorFromHSV(hueF, 1.0, 1.0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
