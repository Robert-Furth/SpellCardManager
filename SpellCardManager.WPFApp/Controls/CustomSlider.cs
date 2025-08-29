using System.Windows.Controls;
using System.Windows.Input;

namespace SpellCardManager.WPFApp.Controls;

/// <summary>
/// Version of the WPF Slider control with better click-drag/IsMoveToPointEnabled behavior.
/// Source: https://stackoverflow.com/a/2960271
/// </summary>
public class CustomSlider : Slider {
    protected override void OnPreviewMouseMove(MouseEventArgs e) {
        if (e.LeftButton == MouseButtonState.Pressed) {
            var fakeEvent = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left) {
                RoutedEvent = PreviewMouseLeftButtonDownEvent,
                Source = e.Source,
            };
            RaiseEvent(fakeEvent);
        }
    }
}