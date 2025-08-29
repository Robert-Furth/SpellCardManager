using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SpellCardManager.WPFApp.Converters;

[ValueConversion(typeof(System.Drawing.Color), typeof(Brush))]
class ColorToBrushConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is not System.Drawing.Color color) return DependencyProperty.UnsetValue;

        return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}

class ColorToColorConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is not System.Drawing.Color color) return DependencyProperty.UnsetValue;

        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is not Color color) return DependencyProperty.UnsetValue;

        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}