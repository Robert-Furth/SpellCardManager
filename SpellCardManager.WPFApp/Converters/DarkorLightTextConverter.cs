using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SpellCardManager.WPFApp.Converters;

internal class DarkOrLightTextConverter : IValueConverter {

    /// <summary>
    /// Calculates the relative luminance of a color, as defined 
    /// <see href="https://www.w3.org/TR/WCAG20/#relativeluminancedef">by the WCAG</see>. Assumes an
    /// sRGB colorspace. Does not account for transparency.
    /// </summary>
    /// <param name="color">The color to calculate the luminance for.</param>
    /// <returns>The relative lumninace as a value between 0 and 1.</returns>
    private static double RelativeLuminance(System.Drawing.Color color) {
        double r = (double)color.R / 255;
        double g = (double)color.G / 255;
        double b = (double)color.B / 255;

        double rlin = r <= 0.03928 ? r / 12.92 : Math.Pow((r + 0.055) / 1.055, 2.4);
        double glin = g <= 0.03928 ? g / 12.92 : Math.Pow((g + 0.055) / 1.055, 2.4);
        double blin = b <= 0.03928 ? b / 12.92 : Math.Pow((b + 0.055) / 1.055, 2.4);

        return 0.2126 * rlin + 0.7152 * glin + 0.0722 * blin;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value is not System.Drawing.Color color) return DependencyProperty.UnsetValue;

        var relLum = RelativeLuminance(color);
        var contrastWhite = 1.05 / (relLum + 0.05);
        //var contrastBlack = (relLum + 0.05) / 0.05;

        return contrastWhite >= 3 ? Brushes.White : Brushes.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}

