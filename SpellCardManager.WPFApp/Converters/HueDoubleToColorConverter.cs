using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SpellCardManager.WPFApp.Converters;

class HueDoubleToColorConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is not double hueF) return DependencyProperty.UnsetValue;

        // Clamp hue to range [0, 1)
        hueF = Math.Clamp(hueF, 0, 1);
        if (hueF == 1) hueF = 0;

        // https://en.wikipedia.org/wiki/HSL_and_HSV#HSV_to_RGB
        double huePrime = hueF * 6;
        double x = 1 - Math.Abs(huePrime % 2 - 1);

        (double rf, double gf, double bf) = huePrime switch {
            < 1 => (1.0, x, 0.0),
            < 2 => (x, 1.0, 0.0),
            < 3 => (0.0, 1.0, x),
            < 4 => (0.0, x, 1.0),
            < 5 => (x, 0.0, 1.0),
            < 6 => (1.0, 0.0, x),

            _ => throw new Exception(),
        };

        byte rb = (byte)(rf * 255);
        byte gb = (byte)(gf * 255);
        byte bb = (byte)(bf * 255);
        return Color.FromRgb(rb, gb, bb);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}
