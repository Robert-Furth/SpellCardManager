using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SpellCardManager.Backend.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace SpellCardManager.ViewModels;

public partial class TagEditorViewModel : ViewModelBase {
    [Reactive] private CardCollection _deck;
    [Reactive] private Tag? _selectedTag;
    [Reactive] private int _selectedIndex = -1;
    [ObservableAsProperty] private bool _isAnySelected;

    public ObservableCollection<Tag> Tags { get; }
    [Reactive] private string? _searchTerm;
    [BindableDerivedList] private readonly ReadOnlyObservableCollection<Tag> _filteredTags;

    protected List<Tag> Removals { get; } = [];
    protected List<Tag> Additions { get; } = [];

    public TagEditorViewModel(CardCollection deck) {
        Deck = deck;

        var tagsEnumerable = deck.Tags.Items
            .OrderBy(tag => tag.Name)
            .Select(tag => tag.Clone());
        Tags = new(tagsEnumerable);

        _isAnySelectedHelper = this
            .WhenAnyValue(x => x.SelectedIndex, idx => idx > -1)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, nameof(IsAnySelected), scheduler: Scheduler.Immediate);

        var tagsFilter = this
            .WhenAnyValue(x => x.SearchTerm)
            .Select(s => (Func<Tag, bool>)(tag => {
                return tag.Name.StartsWith(s ?? "", StringComparison.CurrentCultureIgnoreCase);
            }));

        Tags.ToObservableChangeSet()
            .Filter(tagsFilter)
            .Sort(Comparer<Tag>.Create((tag1, tag2) => tag1.Name.CompareTo(tag2.Name)))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _filteredTags)
            .Subscribe();
    }

#if DEBUG

    private static CardCollection DebugDeck {
        get {
            var deck = new CardCollection();
            deck.Tags.AddOrUpdate([
                new Tag { Name = "Combat", Color = Color.Red },
                new Tag { Name = "Healing", Color= Color.Green },
            ]);

            return deck;
        }
    }

    public TagEditorViewModel() : this(DebugDeck) {
        SelectedIndex = 0;
        SelectedTag = Tags[0];
    }

#endif

    [ReactiveCommand]
    private void AddTag() {
        var _newTag = new Tag { Name = "New Tag", Color = Color.Red };

        Additions.Add(_newTag);
        Tags.Add(_newTag);
        SearchTerm = "";
        SelectedTag = _newTag;
    }

    [ReactiveCommand]
    private void RemoveTag() {
        if (SelectedTag is null) return;

        Removals.Add(SelectedTag);

        // Ensure the index is not reset once the tag is removed
        var index = SelectedIndex;
        NotifyCollectionChangedEventHandler? handler = null;

        // ARGH https://stackoverflow.com/questions/2058176/
        var asIncc = (INotifyCollectionChanged)FilteredTags;
        asIncc.CollectionChanged += handler = (sender, args) => {
            asIncc.CollectionChanged -= handler;
            SelectedIndex = Math.Min(index, FilteredTags.Count - 1);
        };

        // Actually remove the tag
        Tags.Remove(SelectedTag);
    }

    [ReactiveCommand]
    private void SaveChanges() {
        foreach (var addedTag in Additions) {
            Deck.Tags.AddOrUpdate(addedTag);
        }

        foreach (var removedTag in Removals) {
            Deck.RemoveTag(removedTag.Id);
        }

        foreach (var tag in Tags) {
            var maybeTag = Deck.Tags.Lookup(tag.Id);
            if (maybeTag.HasValue) {
                maybeTag.Value.Update(tag);
            }
        }

        Removals.Clear();
    }
}
