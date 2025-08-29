using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpellCardManager.WPFApp.Views;

/// <summary>
/// Interaction logic for SpellViewer.xaml
/// </summary>
public partial class SpellViewer : UserControl {
    public SpellViewer() {
        InitializeComponent();
    }

    private void FlowDocumentScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
        const double SCROLL_SCALE = 2.0;
        if (sender is not FlowDocumentScrollViewer) return;

        var offset = OuterScrollView.VerticalOffset - (e.Delta / 6 * SCROLL_SCALE);
        var scrollDest = Math.Clamp(0, offset, OuterScrollView.ExtentHeight);
        OuterScrollView.ScrollToVerticalOffset(scrollDest);
        e.Handled = true;
    }

    private void MDRender(object sender, RoutedEventArgs e) {
        /*var vm = (SpellTabViewModel)DataContext;

        var renderer = new FlowDocumentRenderer(DescriptionViewer.Document, true);
        var pipeline = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseListExtras()
            .DisableHtml()
            .Build();
        var result = (FlowDocument)Markdig.Markdown.Convert(vm.Card.Description, renderer, pipeline);
        DescriptionViewer.Document = result;
        Debug.WriteLine(result);*/
    }
}
