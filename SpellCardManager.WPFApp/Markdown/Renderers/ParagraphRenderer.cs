using System.Windows.Documents;
using Md = Markdig.Syntax;

namespace SpellCardManager.WPFApp.Markdown.Renderers;

internal class ParagraphRenderer : FlowDocumentObjectRenderer<Md.ParagraphBlock> {
    protected override void Write(FlowDocumentRenderer renderer, Md.ParagraphBlock obj) {
        var paragraph = new Paragraph();
        renderer.Write(paragraph);
        renderer.PushTarget(paragraph);
        renderer.WriteLeafInline(obj);
        renderer.PopTarget();
    }
}
