using Markdig.Syntax.Inlines;
using System.Windows.Documents;

namespace SpellCardManager.WPFApp.Markdown.Renderers;

internal class LiteralInlineRenderer : FlowDocumentObjectRenderer<LiteralInline> {
    protected override void Write(FlowDocumentRenderer renderer, LiteralInline obj) {
        var run = new Run(obj.Content.ToString());
        renderer.Write(run);
    }
}
