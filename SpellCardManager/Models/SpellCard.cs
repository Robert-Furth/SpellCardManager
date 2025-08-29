using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json.Serialization;

namespace SpellCardManager.Backend.Models;

public partial class SpellCard : ReactiveObject {
    public required string Name {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    private string _name = "";

    /// <summary>
    /// Raw text description; should be rendered using Markdown.
    /// </summary>
    [Reactive] private string _description = "";

    [Reactive] private ObservableCollection<KeyValuePair<string, string>> _attributes = [];

    [Reactive] private ObservableCollection<Tag> _tags = [];

    //[Reactive] private bool _isFavorite = false;
    private bool _isFavorite = false;
    public bool IsFavorite {
        get => _isFavorite;
        set => this.RaiseAndSetIfChanged(ref _isFavorite, value);
    }


    [JsonIgnore]
    public Guid Id { get; init; } = Guid.NewGuid();

    [JsonIgnore]
    [BindableDerivedList]
    private readonly ReadOnlyObservableCollection<Tag> _sortedTags;

    [JsonIgnore]
    [ObservableAsProperty]
    private string _level = "[none]";


    /// <summary>
    /// Fires whenever a property or an item in one of the ObservableCollections changed.
    /// </summary>
    [JsonIgnore]
    public IObservable<Unit> RecursiveChange { get; set; }

    public SpellCard() {
        var attributesChanges = this
            .WhenAnyValue(x => x.Attributes, attributes => attributes.ToObservableChangeSet())
            .Switch()
            .Publish();

        var tagsChanges = this
            .WhenAnyValue(x => x.Tags, tags => tags.ToObservableChangeSet())
            .Switch()
            .Publish();

        RecursiveChange = Observable
            .Merge(
                Changed.Select(_ => Unit.Default),
                attributesChanges.SkipInitial().Select(_ => Unit.Default),
                tagsChanges.Select(_ => Unit.Default))
            .AsObservable();

        _levelHelper = attributesChanges
            .Filter(pair => pair.Key.Equals("level", StringComparison.CurrentCultureIgnoreCase))
            .Select(changeSet => {
                if (changeSet.Count == 1) {
                    var change = changeSet.First();
                    if (change.Item.Current.Value is not null)
                        return change.Item.Current.Value;
                    else if (change.Range.Count > 0 && change.Range.First().Value is not null)
                        return change.Range.First().Value;
                }
                return "[none]";
            })
            .ToProperty(this, nameof(Level));

        tagsChanges
            .Sort(Comparer<Tag>.Create((tag1, tag2) => tag1.Name.CompareTo(tag2.Name)))
            .Bind(out _sortedTags)
            .Subscribe();

        attributesChanges.Connect();
        tagsChanges.Connect();
    }

    public string? GetAttribute(string key) {
        foreach (var pair in Attributes) {
            if (pair.Key.ToLower().Equals(key.ToLower())) return pair.Value;
        }
        return null;
    }

    public SpellCard CloneWithCurrentId() => new() {
        Name = Name,
        Description = Description,
        Attributes = new([.. Attributes]),
        Tags = new([.. Tags]),
        Id = Id,
    };

    public SpellCard CloneWithNewId() => new() {
        Name = Name,
        Description = Description,
        Attributes = new([.. Attributes]),
        Tags = new([.. Tags]),
        Id = Guid.NewGuid(),
    };
}

