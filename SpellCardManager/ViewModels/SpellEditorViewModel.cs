using DynamicData;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SpellCardManager.Backend.Models;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;


namespace SpellCardManager.ViewModels;
public class Pair {
    public string Key { get; set; }
    public string Value { get; set; }

    public Pair(string key, string value) {
        Key = key;
        Value = value;
    }
}

public partial class SpellEditorViewModel : ViewModelBase {
    [Reactive] private string _spellName = "";
    [Reactive] private string _spellDescription = "";
    [Reactive] private string _tagBoxContent = "";
    public ObservableCollection<Pair> Attributes { get; } = [];
    public ObservableCollection<Tag> Tags { get; } = [];

    public CardCollection Deck { get; }
    private SpellCard? _origCard;
    private bool _cardIsInDeck;

    [BindableDerivedList]
    private readonly ReadOnlyObservableCollection<string> _availableTags;
    //[Reactive] private ObservableCollection<string> _availableTags;

    [Reactive] private int _selectedTableRow = 0;

    [ObservableAsProperty] private bool _canSave = true;

    public SpellEditorViewModel(CardCollection deck) {
        Deck = deck;
        _cardIsInDeck = false;

        Deck.Tags
            .Connect()
            .AutoRefresh(tag => tag.Name)
            .Transform(tag => tag.Name, transformOnRefresh: true)
            .Bind(out _availableTags)
            .Subscribe();

        _canSaveHelper = this
            .WhenAnyValue(x => x.SpellName)
            .Select(name => name != "")
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, nameof(CanSave), scheduler: Scheduler.Immediate);
    }

    public SpellEditorViewModel(CardCollection deck, SpellCard card) : this(deck) {
        _origCard = card;
        _cardIsInDeck = deck.Cards.Lookup(card.Id).HasValue;

        SpellName = card.Name;
        SpellDescription = card.Description;
        Attributes = [.. card.Attributes.Select(kvp => new Pair(kvp.Key, kvp.Value))];
        Tags = [.. card.Tags];
    }

    private SpellCard ToCard() => new() {
        Name = SpellName,
        Description = SpellDescription,
        Attributes = [.. Attributes.Select(pair => new KeyValuePair<string, string>(
            pair.Key, pair.Value))],
        Tags = [.. Tags],
        Id = _origCard?.Id ?? Guid.NewGuid(),
    };

    #region Commands

    [ReactiveCommand]
    private void MoveRowUp() {
        var row = SelectedTableRow;
        if (row >= 1 && row < Attributes.Count) {
            (Attributes[row], Attributes[row - 1]) = (Attributes[row - 1], Attributes[row]);
            SelectedTableRow = row - 1;
        }
    }

    [ReactiveCommand]
    private void MoveRowDown() {
        var row = SelectedTableRow;
        if (row >= 0 && row < Attributes.Count - 1) {
            (Attributes[row], Attributes[row + 1]) = (Attributes[row + 1], Attributes[row]);
            SelectedTableRow = row + 1;
        }
    }

    [ReactiveCommand]
    private void AddAttributeRow() {
        Attributes.Add(new("", ""));
        SelectedTableRow = Attributes.Count - 1;
    }

    [ReactiveCommand]
    private void RemoveAttributeRow() {
        var row = SelectedTableRow;
        if (row >= 0 && row < Attributes.Count) {
            Attributes.RemoveAt(row);
        }
    }

    [ReactiveCommand]
    private void AddTag() {
        foreach (var tag in Deck.Tags.Items) {
            if (tag is not null && tag.Name == TagBoxContent && !Tags.Contains(tag)) {
                Tags.Add(tag);
                TagBoxContent = "";
                break;
            }
        }
    }

    [ReactiveCommand]
    private void RemoveTag(string tagName) {
        for (var i = 0; i < Tags.Count; i++) {
            if (Tags[i].Name == tagName) {
                Tags.RemoveAt(i);
                break;
            }
        }
    }

    [ReactiveCommand]
    private async Task EditTags() => await Interactions.OpenTagEditor.Handle(Unit.Default);

    [ReactiveCommand]
    private void SaveChanges() {
        if (!CanSave) return;

        if (_cardIsInDeck && _origCard is not null) {
            _origCard.Name = SpellName;
            _origCard.Description = SpellDescription;

            _origCard.Attributes = new(
                Attributes.Select(pair => new KeyValuePair<string, string>(pair.Key, pair.Value)));

            _origCard.Tags = new(Tags);
        } else {
            _origCard = ToCard();
            Deck.Cards.AddOrUpdate(_origCard);
            _cardIsInDeck = true;
        }
    }

    #endregion
}
