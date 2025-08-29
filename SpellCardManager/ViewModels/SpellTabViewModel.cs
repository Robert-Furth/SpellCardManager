//using Markdig.Syntax;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SpellCardManager.Backend.Models;
using SpellCardManager.Services;
using System.Drawing;


namespace SpellCardManager.ViewModels;

public partial class SpellTabViewModel : ViewModelBase {
    #region Properties

    [Reactive] private SpellCard _card;
    [Reactive] private bool _isTemporary = true;

    [ObservableAsProperty] private string _name = "";
    [ObservableAsProperty] private object? _renderedMarkdown;

    #endregion

    public SpellTabViewModel(SpellCard card, IMarkdownRenderService? markdownRenderService) {
        Card = card;

        _nameHelper = this
            .WhenAnyValue(x => x.Card.Name)
            .ToProperty(this, nameof(Name));

        _renderedMarkdownHelper = this
            .WhenAnyValue(x => x.Card.Description, desc => markdownRenderService?.Render(desc))
            .ToProperty(this, nameof(RenderedMarkdown));
    }
}

#if DEBUG

public class SpellTabViewModelDesign : SpellTabViewModel {
    public SpellTabViewModelDesign() : base(new() {
        Name = "Foo",
        Tags = [
        new() { Name = "Combat", Color = Color.Firebrick },
            new() { Name = "Utility", Color = Color.Gold },
        ],
        Attributes = [
        new("Level", "1"),
            new("Components", "V S"),
        ],
        Description = "According to all known laws of aviation...",
    }, null!) {

    }
}

#endif