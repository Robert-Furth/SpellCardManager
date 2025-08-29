using DynamicData;
using Markdig.Renderers;
using SpellCardManager.WPFApp.Markdown.Renderers;
using System.Windows.Documents;
using Md = Markdig.Syntax;

namespace SpellCardManager.WPFApp.Markdown;

internal class FlowDocumentRenderer : RendererBase {
    private readonly FlowDocument _document = new();
    private readonly Stack<TextElement> _targets = new();

    public FlowDocumentRenderer(FlowDocument doc, bool clear = false) {
        _document = doc;
        if (clear) {
            _document.Blocks.Clear();
        }

        ObjectRenderers.Add([
            // Blocks
            new HeadingRenderer(),
            new ListRenderer(),
            new ParagraphRenderer(),
            new TableRenderer(),

            // Inlines
            new EmphasisInlineRenderer(),
            new LiteralInlineRenderer(),
        ]);
    }

    public FlowDocumentRenderer() : this(new FlowDocument()) { }

    public override object Render(Md.MarkdownObject markdownObject) {
        Write(markdownObject);
        return _document;
    }

    private BlockCollection CurBlockCollection() {
        if (_targets.Count == 0) return _document.Blocks;

        return _targets.Peek() switch {
            Section s => s.Blocks,
            ListItem li => li.Blocks,
            TableCell td => td.Blocks,
            _ => throw NewChildTypeException(_targets.Peek().GetType(), "block"),
        };
    }

    private InlineCollection CurInlineCollection() {
        if (_targets.Count == 0) throw NewChildTypeException(_document.GetType(), "inline");

        return _targets.Peek() switch {
            Paragraph p => p.Inlines,
            Span s => s.Inlines,
            _ => throw NewChildTypeException(_targets.Peek().GetType(), "inline")
        };
    }

    private ListItemCollection CurListItemCollection() {
        if (_targets.Count == 0) throw NewChildTypeException(_document.GetType(), "list item");

        return _targets.Peek() switch {
            System.Windows.Documents.List ls => ls.ListItems,
            _ => throw NewChildTypeException(_targets.Peek().GetType(), "list item")
        };
    }

    private static InvalidOperationException NewChildTypeException(Type t, string blockType) => new(
        $"Tried to get the child {blockType}s of a {t}, which does not have child {blockType}s");

    public void PushTarget(TextElement target) {
        _targets.Push(target);
    }

    public TextElement PopTarget() {
        return _targets.Pop();
    }

    public void Write(Inline inline) {
        var collection = CurInlineCollection();
        collection.Add(inline);
    }

    public void Write(Block block) {
        var collection = CurBlockCollection();
        collection.Add(block);
    }

    public void Write(ListItem listItem) {
        var collection = CurListItemCollection();
        collection.Add(listItem);
    }

    public void WriteLeafInline(Md.LeafBlock leafBlock) {
        Md.Inlines.Inline? inline = leafBlock.Inline;
        while (inline is not null) {
            Write(inline);
            inline = inline.NextSibling;
        }
    }
}
