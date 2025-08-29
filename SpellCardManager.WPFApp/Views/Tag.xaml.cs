using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SpellCardManager.WPFApp.Views;

/// <summary>
/// Interaction logic for Tag.xaml
/// </summary>
public partial class Tag : UserControl {

    public Brush TagBackground {
        get => (Brush)GetValue(TagBackgroundProperty);
        set => SetValue(TagBackgroundProperty, value);
    }

    public Brush Stroke {
        get => (Brush)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public string Text {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Visibility CloseButtonVisiblity {
        get => (Visibility)GetValue(CloseButtonVisiblityProperty);
        set => SetValue(CloseButtonVisiblityProperty, value);
    }

    public ICommand CloseCommand {
        get => (ICommand)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    public object? CloseCommandParameter {
        get => (object?)GetValue(CloseCommandParameterProperty);
        set => SetValue(CloseCommandParameterProperty, value);
    }

    public static readonly DependencyProperty TagBackgroundProperty =
        DependencyProperty.Register(
            nameof(TagBackground),
            typeof(Brush),
            typeof(Tag),
            new FrameworkPropertyMetadata(Brushes.Red));

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(Tag));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(Tag));

    public static readonly DependencyProperty CloseButtonVisiblityProperty =
        DependencyProperty.Register(
            nameof(CloseButtonVisiblity),
            typeof(Visibility),
            typeof(Tag),
            new FrameworkPropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(Tag));

    public static readonly DependencyProperty CloseCommandParameterProperty =
        DependencyProperty.RegisterAttached(nameof(CloseCommandParameter), typeof(object), typeof(Tag));
    public Tag() {
        InitializeComponent();
    }
}
