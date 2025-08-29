using System.Windows;
using System.Windows.Documents;
using Md = Markdig.Syntax;

namespace SpellCardManager.WPFApp.Markdown.Renderers;

class EmphasisInlineRenderer : FlowDocumentObjectRenderer<Md.Inlines.EmphasisInline> {
    protected override void Write(FlowDocumentRenderer renderer, Md.Inlines.EmphasisInline obj) {
        var span = new Span();

        if (obj.DelimiterChar is '*' or '_') {
            if (obj.DelimiterCount == 2) {
                span.FontWeight = FontWeights.Bold;
            } else {
                span.FontStyle = FontStyles.Italic;
            }
        }

        renderer.Write(span);
        renderer.PushTarget(span);
        renderer.WriteChildren(obj);
        renderer.PopTarget();
    }
}
