using Markdig.Syntax;
using System.Windows;
using System.Windows.Documents;
using Md = Markdig.Syntax;

namespace SpellCardManager.WPFApp.Markdown.Renderers;

internal class HeadingRenderer : FlowDocumentObjectRenderer<Md.HeadingBlock> {
    protected override void Write(FlowDocumentRenderer renderer, HeadingBlock obj) {
        var resources = (ResourceDictionary)Application.LoadComponent(
            new("Styles/Text.xaml", UriKind.Relative));

        var p = new Paragraph {
            Style = (Style)resources["ParagraphH1Style"]
        };

        renderer.Write(p);
        renderer.PushTarget(p);
        renderer.WriteLeafInline(obj);
        renderer.PopTarget();
    }
}
