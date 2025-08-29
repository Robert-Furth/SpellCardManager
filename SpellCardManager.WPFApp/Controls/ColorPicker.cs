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

[TemplatePart(Name = "PART_SaturationValueArea", Type = typeof(Grid))]
public partial class ColorPicker : Control {
    private Grid? _satValArea;
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
            OnSelectedColorPropertyChanged));

    public Color SelectedColor {
        get => (Color)GetValue(SelectedColorProperty);
        set {
            SetValue(SelectedColorProperty, value);
        }
    }

    private static void OnSelectedColorPropertyChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if (d is not ColorPicker p) return;
        p._colorChanging = true;

        try {
            if (!p._hsvChanging) {
                var (h, s, v) = ((Color)e.NewValue).GetHSV();
                p.SelectedHue = h;
                p.SelectedSaturation = s;
                p.SelectedValue = v;
            }

            p.UpdateThumb();
        } finally {
            p._colorChanging = false;
        }
    }

    public static readonly DependencyProperty SelectedHueProperty = DependencyProperty.Register(
        nameof(SelectedHue),
        typeof(double),
        typeof(ColorPicker),
        new FrameworkPropertyMetadata(
            0.0D,
            FrameworkPropertyMetadataOptions.AffectsRender
            | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSelectedHSVPropertiesChanged));

    public double SelectedHue {
        get => (double)GetValue(SelectedHueProperty);
        set => SetValue(SelectedHueProperty, value);
    }

    private static void OnSelectedHSVPropertiesChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e) {

        if (d is not ColorPicker p) return;
        p._hsvChanging = true;
        try {
            if (!p._colorChanging) {
                var newColor = ColorExtensions.ColorFromHSV(
                    p.SelectedHue, p.SelectedSaturation, p.SelectedValue);
                p.SelectedColor = newColor;
            }
        } finally {
            p._hsvChanging = false;
        }
    }

    public static readonly DependencyProperty SelectedSaturationProperty = DependencyProperty.Register(
        nameof(SelectedSaturation),
        typeof(double),
        typeof(ColorPicker),
        new FrameworkPropertyMetadata(
            0.0D,
            FrameworkPropertyMetadataOptions.AffectsRender
            | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSelectedHSVPropertiesChanged));

    public double SelectedSaturation {
        get => (double)GetValue(SelectedSaturationProperty);
        set => SetValue(SelectedSaturationProperty, value);
    }



    public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
        nameof(SelectedValue),
        typeof(double),
        typeof(ColorPicker),
        new FrameworkPropertyMetadata(
            0.0D,
            FrameworkPropertyMetadataOptions.AffectsRender
            | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnSelectedHSVPropertiesChanged));

    public double SelectedValue {
        get => (double)GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /*public static readonly DependencyPropertyKey ThumbXPropertyKey
        = DependencyProperty.RegisterReadOnly(
            nameof(ThumbX),
            typeof(GridLength),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star)));

    public GridLength ThumbX => (GridLength)GetValue(ThumbXPropertyKey.DependencyProperty);

    public static readonly DependencyPropertyKey ThumbYPropertyKey
        = DependencyProperty.RegisterReadOnly(
            nameof(ThumbY),
            typeof(GridLength),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star)));

    public GridLength ThumbY => (GridLength)GetValue(ThumbYPropertyKey.DependencyProperty);

    public static readonly DependencyPropertyKey ThumbX2PropertyKey
        = DependencyProperty.RegisterReadOnly(
            nameof(ThumbX2),
            typeof(GridLength),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star)));

    public GridLength ThumbX2 => (GridLength)GetValue(ThumbX2PropertyKey.DependencyProperty);

    public static readonly DependencyPropertyKey ThumbY2PropertyKey
        = DependencyProperty.RegisterReadOnly(
            nameof(ThumbY2),
            typeof(GridLength),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(new GridLength(2, GridUnitType.Star)));

    public GridLength ThumbY2 => (GridLength)GetValue(ThumbY2PropertyKey.DependencyProperty);*/

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
        SelectedSaturation = saturation;
        SelectedValue = value;

        var p2 = new Point(
            Math.Clamp(p.X, 0, _satValArea.ActualWidth),
            Math.Clamp(p.Y, 0, _satValArea.ActualHeight));
        UpdateThumb(p2);
    }

    private void UpdateThumb() {
        if (_satValArea is null) return;

        var x = SelectedSaturation;
        var y = SelectedValue;

        _satValArea.ColumnDefinitions[0].Width = new GridLength(x, GridUnitType.Star);
        _satValArea.ColumnDefinitions[1].Width = new GridLength(1 - x, GridUnitType.Star);
        _satValArea.RowDefinitions[0].Height = new GridLength(1 - y, GridUnitType.Star);
        _satValArea.RowDefinitions[1].Height = new GridLength(y, GridUnitType.Star);
    }

    private void UpdateThumb(Point p) {
        if (_satValArea is null) return;

        var x = p.X / _satValArea.ActualWidth;
        var y = p.Y / _satValArea.ActualHeight;

        _satValArea.ColumnDefinitions[0].Width = new GridLength(x, GridUnitType.Star);
        _satValArea.ColumnDefinitions[1].Width = new GridLength(1 - x, GridUnitType.Star);
        _satValArea.RowDefinitions[0].Height = new GridLength(y, GridUnitType.Star);
        _satValArea.RowDefinitions[1].Height = new GridLength(1 - y, GridUnitType.Star);
    }

    #endregion

    public override void OnApplyTemplate() {
        base.OnApplyTemplate();

        _satValArea = (Grid)GetTemplateChild("PART_SaturationValueArea");
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
