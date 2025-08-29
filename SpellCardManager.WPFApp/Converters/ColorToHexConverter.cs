using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SpellCardManager.WPFApp.Converters;

internal partial class ColorToHexConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is not Color color) return DependencyProperty.UnsetValue;

        return $"#{color.R:x2}{color.G:x2}{color.B:x2}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is not string s) return DependencyProperty.UnsetValue;

        var match = ColorStringRegex().Match(s);
        if (!match.Success) return DependencyProperty.UnsetValue;

        var hexString = match.Groups[1].Value!;
        var r = System.Convert.ToByte(hexString[0..2], 16);
        var g = System.Convert.ToByte(hexString[2..4], 16);
        var b = System.Convert.ToByte(hexString[4..6], 16);
        return Color.FromRgb(r, g, b);
    }

    [GeneratedRegex(@"^\s*#?([0-9A-Fa-f]{6})\s*$")]
    private static partial Regex ColorStringRegex();
}
