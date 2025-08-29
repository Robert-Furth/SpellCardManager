using Markdig;
using SpellCardManager.Services;
using SpellCardManager.WPFApp.Markdown;
using System.Windows;
using System.Windows.Documents;

namespace SpellCardManager.WPFApp.Services;

internal class MarkdownRenderService : IMarkdownRenderService {

    private readonly MarkdownPipeline _pipeline;

    public MarkdownRenderService() {
        _pipeline = new MarkdownPipelineBuilder()
            .UsePipeTables()
            .UseListExtras()
            .DisableHtml()
            .Build();
    }

    public object Render(string markdownText) {
        var renderer = new FlowDocumentRenderer();
        var document = (FlowDocument)Markdig.Markdown.Convert(markdownText, renderer, _pipeline);

        document.SetResourceReference(
            FlowDocument.FontFamilyProperty, SystemFonts.MessageFontFamilyKey);
        document.SetResourceReference(
            FlowDocument.FontSizeProperty, "ControlContentThemeFontSize");
        document.PagePadding = new Thickness(0);

        return document;
    }
}
