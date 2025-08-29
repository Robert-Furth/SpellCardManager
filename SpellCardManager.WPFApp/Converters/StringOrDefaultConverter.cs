using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SpellCardManager.WPFApp.Converters;

internal class StringOrDefaultConverter : IValueConverter {
    public bool VisualIsEmpty { get; set; } = false;

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) {
        if (value is not string s) return DependencyProperty.UnsetValue;

        return VisualIsEmpty ? "" : s;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    ) {
        if (value is not string s) return DependencyProperty.UnsetValue;

        if (s.Length == 0) {
            VisualIsEmpty = true;
            return parameter;
        }
        VisualIsEmpty = false;
        return s;

    }
}
