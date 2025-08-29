using Markdig.Renderers;
using Markdig.Syntax;

namespace SpellCardManager.WPFApp.Markdown;

internal abstract class FlowDocumentObjectRenderer<T>
    : MarkdownObjectRenderer<FlowDocumentRenderer, T> where T : MarkdownObject { }
