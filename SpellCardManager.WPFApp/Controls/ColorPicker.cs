using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SpellCardManager.WPFApp.Controls;

/// <summary>
/// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
///
/// Step 1a) Using this custom control in a XAML file that exists in the current project.
/// Add this XmlNamespace attribute to the root element of the markup file where it is 
/// to be used:
///
///     xmlns:MyNamespace="clr-namespace:SpellCardManager.WPFApp.Controls"
///
///
/// Step 1b) Using this custom control in a XAML file that exists in a different project.
/// Add this XmlNamespace attribute to the root element of the markup file where it is 
/// to be used:
///
///     xmlns:MyNamespace="clr-namespace:SpellCardManager.WPFApp.Controls;assembly=SpellCardManager.WPFApp.Controls"
///
/// You will also need to add a project reference from the project where the XAML file lives
/// to this project and Rebuild to avoid compilation errors:
///
///     Right click on the target project in the Solution Explorer and
///     "Add Reference"->"Projects"->[Browse to and select this project]
///
///
/// Step 2)
/// Go ahead and use your control in the XAML file.
///
///     <MyNamespace:CustomControl1/>
///
/// </summary>
/// 

[TemplatePart(Name = "PART_SaturationValueArea", Type = typeof(Control))]
public partial class ColorPicker : Control {
    private FrameworkElement? _satValArea;
    private FrameworkElement? _satValAreaThumb;

    private bool _mouseCaptured = false;

    private bool _colorChanging = false;
    private bool _hsvChanging = false;


    #region Properties

    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
        nameof(SelectedColor),
        typeof(Color?),
        typeof(ColorPicker),
        new FrameworkPropertyMetadata(
            Colors.White,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
            | FrameworkPropertyMetadataOptions.AffectsRender,
            OnSelectedColorPropertyChanged,
            CoerceColor));

    public Color SelectedColor {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    private static object CoerceColor(DependencyObject d, object value) {
        switch (value) {
            case Color c:
                return c;
            case string s:
                var match = ColorStringRegex().Match(s);
                if (!match.Success) return Colors.White;

                var hexString = match.Groups[1].Value!;
                var r = Convert.ToByte(hexString[0..2], 16);
                var g = Convert.ToByte(hexString[2..4], 16);
                var b = Convert.ToByte(hexString[4..6], 16);
                return Color.FromRgb(r, g, b);

            default:
                return Colors.White;
        }
    }

    private static void OnSelectedColorPropertyChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e) {

        if (d is not ColorPicker picker || picker._colorChanging) return;
        picker._colorChanging = true;

        if (!picker._hsvChanging) {
            var newColor = (Color)e.NewValue;
            var (h, s, v) = newColor.GetHSV();
            picker.SetCurrentValue(SelectedHueProperty, h);
            picker.SetCurrentValue(SelectedSaturationProperty, s);
            picker.SetCurrentValue(SelectedValueProperty, v);
        }

        picker.UpdateThumb();
        picker._colorChanging = false;
    }

    public static readonly DependencyProperty SelectedHueProperty = DependencyProperty.Register(
        nameof(SelectedHue),
        typeof(double),
        typeof(ColorPicker),
        new FrameworkPropertyMetadata(
            0.0D,
            FrameworkPropertyMetadataOptions.AffectsRender,
            OnSelectedHuePropertyChanged));

    public double SelectedHue {
        get => (double)GetValue(SelectedHueProperty);
        set => SetValue(SelectedHueProperty, value);
    }

    private static void OnSelectedHuePropertyChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e) {

        if (d is not ColorPicker picker || picker._hsvChanging) return;
        picker._hsvChanging = true;

        double newHue = (double)e.NewValue;
        if (!picker._colorChanging) {
            var newColor = Extensions.ColorFromHSV(
                newHue, picker.SelectedSaturation, picker.SelectedValue);
            picker.SetCurrentValue(SelectedColorProperty, newColor);
        }

        picker._hsvChanging = false;
    }

    public static readonly DependencyProperty SelectedSaturationProperty = DependencyProperty.Register(
        nameof(SelectedSaturation),
        typeof(double),
        typeof(ColorPicker),
        new FrameworkPropertyMetadata(
            0.0D,
            FrameworkPropertyMetadataOptions.AffectsRender,
            OnSelectedSaturationPropertyChanged));

    public double SelectedSaturation {
        get => (double)GetValue(SelectedSaturationProperty);
        set => SetValue(SelectedSaturationProperty, value);
    }

    private static void OnSelectedSaturationPropertyChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e) {

        if (d is not ColorPicker picker || picker._hsvChanging) return;
        picker._hsvChanging = true;

        if (!picker._colorChanging) {
            double newSat = (double)e.NewValue;
            var newColor = Extensions.ColorFromHSV(
                picker.SelectedHue, newSat, picker.SelectedValue);
            picker.SetCurrentValue(SelectedColorProperty, newColor);
        }

        picker._hsvChanging = false;
    }

    public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
        nameof(SelectedValue),
        typeof(double),
        typeof(ColorPicker),
        new FrameworkPropertyMetadata(
            0.0D,
            FrameworkPropertyMetadataOptions.AffectsRender,
            OnSelectedValuePropertyChanged));

    public double SelectedValue {
        get => (double)GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    private static void OnSelectedValuePropertyChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e) {

        if (d is not ColorPicker picker || picker._hsvChanging || picker._colorChanging) return;
        picker._hsvChanging = true;

        if (!picker._colorChanging) {
            double newVal = (double)e.NewValue;
            var newColor = Extensions.ColorFromHSV(
                picker.SelectedHue, picker.SelectedSaturation, newVal);
            picker.SetCurrentValue(SelectedColorProperty, newColor);
        }

        picker._hsvChanging = false;
    }

    #endregion

    #region Event Handlers

    private void PART_SaturationValueArea_MouseLeftButtonDown(
        object sender, MouseButtonEventArgs e) {

        Mouse.Capture(_satValArea);
        _mouseCaptured = true;

        UpdateSV(e.GetPosition(_satValArea));
    }

    private void PART_SaturationValueArea_MouseLeftButtonUp(
        object sender, MouseButtonEventArgs e) {

        _satValArea?.ReleaseMouseCapture();
        _mouseCaptured = false;
    }

    private void PART_SaturationValueArea_MouseMove(object sender, MouseEventArgs e) {
        if (!_mouseCaptured) return;

        var p = e.GetPosition(_satValArea);
        UpdateSV(p);
    }

    private void UpdateSV(Point p) {
        if (_satValArea is null) return;
        if (_satValArea.ActualWidth <= 0 || _satValArea.ActualHeight <= 0) return;

        // saturation left-right, value top-bottom
        double saturation = Math.Clamp(p.X / _satValArea.ActualWidth, 0, 1);
        double value = 1 - Math.Clamp(p.Y / _satValArea.ActualHeight, 0, 1);

        //SetCurrentValue(SelectedColorProperty, Extensions.ColorFromHSV(SelectedHue, saturation, value));
        SetCurrentValue(SelectedSaturationProperty, saturation);
        SetCurrentValue(SelectedValueProperty, value);

        var p2 = new Point(
            Math.Clamp(p.X, 0, _satValArea.ActualWidth),
            Math.Clamp(p.Y, 0, _satValArea.ActualHeight));
        UpdateThumb(p2);
    }

    private void UpdateThumb() {
        if (_satValAreaThumb is null || _satValArea is null) return;

        var x = _satValArea.ActualWidth * SelectedSaturation;
        var y = _satValArea.ActualHeight * (1 - SelectedValue);
        var margin = new Thickness(x, y, 0, 0);
        _satValAreaThumb.Margin = margin;
    }

    private void UpdateThumb(Point p) {
        if (_satValAreaThumb is null) return;
        var margin = new Thickness(p.X, p.Y, 0, 0);
        _satValAreaThumb.Margin = margin;
    }

    #endregion

    public override void OnApplyTemplate() {
        base.OnApplyTemplate();

        _satValArea = (FrameworkElement)GetTemplateChild("PART_SaturationValueArea");
        _satValAreaThumb = (FrameworkElement)GetTemplateChild("SatValAreaThumb");
        if (_satValArea != null) {
            _satValArea.MouseLeftButtonDown += PART_SaturationValueArea_MouseLeftButtonDown;
            _satValArea.MouseLeftButtonUp += PART_SaturationValueArea_MouseLeftButtonUp;
            _satValArea.MouseMove += PART_SaturationValueArea_MouseMove;
        }
    }

    static ColorPicker() {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
    }

    [GeneratedRegex(@"^#([0-9A-Fa-f]{6})$")]
    private static partial Regex ColorStringRegex();
}
