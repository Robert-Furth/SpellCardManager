using Markdig.Syntax;
using System.Windows;
using System.Windows.Documents;
using Md = Markdig.Syntax;

namespace SpellCardManager.WPFApp.Markdown.Renderers;

internal class ListRenderer : FlowDocumentObjectRenderer<Md.ListBlock> {
    protected override void Write(FlowDocumentRenderer renderer, ListBlock obj) {
        var listElement = new List();

        if (obj.IsOrdered) {
            listElement.MarkerStyle = obj.BulletType switch {
                'a' => TextMarkerStyle.LowerLatin,
                'A' => TextMarkerStyle.UpperLatin,
                'i' => TextMarkerStyle.LowerRoman,
                'I' => TextMarkerStyle.UpperRoman,
                _ => TextMarkerStyle.Decimal,
            };

            if (int.TryParse(obj.OrderedStart, out var startIndex)) {
                listElement.StartIndex = startIndex;
            }
        }

        renderer.Write(listElement);
        renderer.PushTarget(listElement);
        foreach (var item in obj) {
            var listItem = new ListItem();
            renderer.Write(listItem);
            renderer.PushTarget(listItem);
            renderer.WriteChildren((Md.ListItemBlock)item);
            renderer.PopTarget();
        }
        renderer.PopTarget();
    }
}
