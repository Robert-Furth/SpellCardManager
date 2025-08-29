using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SpellCardManager.Backend.Models;
using SpellCardManager.Services;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;


namespace SpellCardManager.ViewModels;

public partial class MainViewModel : ViewModelBase {

    #region Properties

    [Reactive] private CardCollection _deck = new();

    #region Tabs

    [Reactive] private int _selectedTab = -1;
    public ObservableCollection<SpellTabViewModel> OpenTabs { get; } = [];
    private readonly IObservable<bool> _tabIsOpen;

    #endregion

    #region Window name

    [Reactive] private Uri? _curFilePath = null;
    [ObservableAsProperty] private string _windowName = "";

    #endregion

    #region Search panel

    [Reactive] private string _searchTerm = "";
    [Reactive] private bool _isSearchWholeWord = false;
    [Reactive] private bool _isSearchCaseSensitive = false;
    [Reactive] private bool _searchDescriptions = true;

    [Reactive] private string _tagSearchBoxText = "";
    [BindableDerivedList] private readonly ReadOnlyObservableCollection<Tag> _sortedTags;
    [BindableDerivedList] private readonly ReadOnlyObservableCollection<string>? _tagNames;

    [Reactive] private bool _groupByLevel = true;
    [Reactive] private bool _favoritesFirst = false;

    public ObservableCollection<Tag> SearchedTags { get; } = [];

    [BindableDerivedList]
    private readonly ReadOnlyObservableCollection<SpellCard> _sortedCards;
    [Reactive] private SpellCard? _selectedCard;
    private readonly IObservable<bool> _selectedCardIsNotNull;


    #endregion

    [Reactive] private bool _isDirty = false;

    #endregion

    #region Services

    private readonly IFileService _fileService;
    private readonly IMessageBoxService _msgBoxService;
    private readonly IMarkdownRenderService _markdownRenderService;

    #endregion

    public MainViewModel(
        IFileService fileService,
        IMessageBoxService messageBoxService,
        IMarkdownRenderService markdownRenderService) {

        _fileService = fileService;
        _msgBoxService = messageBoxService;
        _markdownRenderService = markdownRenderService;

        // Ensure window name is updated
        _windowNameHelper = this
            .WhenAnyValue(
                x => x.CurFilePath, x => x.IsDirty,
                (path, dirty) => {
                    var str = Path.GetFileName(path?.LocalPath) ?? "New Deck";
                    if (dirty) {
                        str = "*" + str;
                    }
                    return str;
                })
            .ToProperty(this, nameof(WindowName));

        _tabIsOpen = this.WhenAnyValue(x => x.SelectedTab, selectedTab => selectedTab >= 0);

        _selectedCardIsNotNull = this
            .WhenAnyValue(x => x.SelectedCard, (SpellCard? card) => card is not null);

        this.WhenAnyValue(x => x.SelectedCard)
            // `card!` can actually be null, but the OpenTab command can handle it.
            // The [ReactiveCommand] annotation doesn't preserve nullability for some reason.
            .Subscribe(card => OpenTabCommand.Execute(card!).Subscribe());

        this.WhenAnyValue(x => x.Deck)
            .Do(deck => {
                // Clear open tabs, searched tags, dirty flag when a new deck is opened
                SelectedCard = null;
                OpenTabs.Clear();
                SearchedTags.Clear();
                IsDirty = false;
            })
            // Whenever cards or tags change, set the dirty flag
            .Select(deck => Observable.Merge(
                deck.Cards.Connect().AutoRefreshOnObservable(x => x.RecursiveChange).SkipInitial().Select(_ => Unit.Default),
                deck.Tags.Connect().AutoRefresh().SkipInitial().Select(_ => Unit.Default)))
            .Switch()
            .Subscribe(_ => {
                IsDirty = true;
            });

        var cardsObservable = this
            .WhenAnyValue(x => x.Deck.Cards, cards => cards.Connect())
            .Switch()
            .Publish();

        var tagsObservable = this
            .WhenAnyValue(x => x.Deck.Tags, tags => tags.Connect())
            .Switch()
            .Publish();

        // Filter for selected tags
        var tagsFilter = SearchedTags
            .ToObservableChangeSet()
            .Select(_ => (Func<SpellCard, bool>)(card => {
                foreach (var tag in SearchedTags) {
                    if (!card.Tags.Contains(tag)) return false;
                }
                return true;
            }));

        // Filter for selected search terms
        var searchTermFilter = this
            .WhenAnyValue(
                x => x.SearchTerm,
                x => x.IsSearchWholeWord,
                x => x.IsSearchCaseSensitive,
                x => x.SearchDescriptions,
                (st, ww, cs, sd) => (st?.Trim() ?? "", ww, cs, sd))
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Select<(string, bool, bool, bool), Func<SpellCard, bool>>(tuple => {
                var searchTerm = tuple.Item1;
                var searchWholeWords = tuple.Item2;
                var caseSensitive = tuple.Item3;
                var searchDescriptions = tuple.Item4;

                if (searchTerm.Length == 0) return card => true;

                if (searchWholeWords) {
                    var regexOptions = RegexOptions.NonBacktracking;
                    if (!caseSensitive) regexOptions |= RegexOptions.IgnoreCase;

                    var re = new Regex(@"\b" + Regex.Escape(searchTerm) + @"\b", regexOptions);
                    return card => re.IsMatch(card.Name)
                        || (searchDescriptions && re.IsMatch(card.Description));
                }

                var comparisonType = caseSensitive
                    ? StringComparison.CurrentCulture
                    : StringComparison.CurrentCultureIgnoreCase;

                return card => card.Name.Contains(searchTerm, comparisonType)
                    || (searchDescriptions && card.Description.Contains(searchTerm, comparisonType));
            });

        // Comparer for sorting the spell list
        var comparerObservable = this
            .WhenAnyValue(
                x => x.GroupByLevel,
                x => x.FavoritesFirst,
                (groupByLevel, favoritesFirst) => Comparer<SpellCard>.Create((card1, card2) => {
                    if (groupByLevel) {
                        var levelComparison = card1.Level.CompareTo(card2.Level);
                        if (levelComparison != 0) return levelComparison;
                    }

                    if (favoritesFirst) {
                        var favoriteComparison = card2.IsFavorite.CompareTo(card1.IsFavorite);
                        if (favoriteComparison != 0) return favoriteComparison;
                    }

                    return card1.Name.CompareTo(card2.Name);
                }));

        // Sorted and filtered cards
        cardsObservable
            .Filter(tagsFilter)
            .Filter(searchTermFilter)
            .AutoRefresh(card => card.Level)
            .AutoRefresh(card => card.IsFavorite)
            .ObserveOn(RxApp.MainThreadScheduler)
            .SortAndBind(out _sortedCards, comparerObservable)
            .Subscribe();

        // Sorted tags
        tagsObservable
            .ObserveOn(RxApp.MainThreadScheduler)
            .SortAndBind(out _sortedTags,
                         Comparer<Tag>.Create((t1, t2) => t1.Name.CompareTo(t2.Name)))
            .Subscribe();

        //var searchFilter = this
        //    .WhenAnyValue(x => x.TagSearchBoxText)
        //    .Select<string, Func<string, bool>>(text => tagName
        //        => tagName.StartsWith(text, StringComparison.CurrentCultureIgnoreCase));

        // Available tag names
        tagsObservable
            .AutoRefresh(tag => tag.Name)
            .Transform(tag => tag.Name, transformOnRefresh: true)
            //.Filter(searchFilter)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _tagNames)
            .Subscribe();

        //var searchedTagsFilter = SearchedTags
        //    .ToObservableChangeSet()
        //    .Select(_ => (Tag tag) => {

        //    });


        cardsObservable.Connect();
        tagsObservable.Connect();
    }

    #region Public Methods

    public async Task<bool> CheckCanClose() {
        if (!IsDirty) return true;
        if (_msgBoxService is null) return true;

        var message = CurFilePath is null
            ? "You have unsaved changes. Would you like to save them?"
            : $"{Path.GetFileName(CurFilePath.LocalPath)} has unsaved changes. Would you like to " +
              "save them?";

        var saveBoxResult = await _msgBoxService.ShowWarningBox(
            "Save", message, MessageBoxButtons.YesNoCancel);

        switch (saveBoxResult) {
            case MessageBoxResult.Yes:
                try {
                    return await SaveImpl(CurFilePath, true) == SaveResult.Success;
                } catch (Exception ex) {
                    var confirmExit = await _msgBoxService.ShowErrorBox(
                        "Error",
                        $"There was an error opening the file:\n{ex.Message}\n" +
                        "Would you like to quit anyway?",
                        MessageBoxButtons.YesNo);
                    return confirmExit == MessageBoxResult.Yes;
                }

            case MessageBoxResult.No:
                return true;

            case MessageBoxResult.None:
            case MessageBoxResult.Cancel:
                return false;

            default:
                throw new UnreachableException();
        }
    }

    public SpellEditorViewModel CreateSpellEditorVM() => new(Deck);
    public SpellEditorViewModel CreateSpellEditorVM(SpellCard card) => new(Deck, card);

    public TagEditorViewModel CreateTagEditorVM() => new(Deck);

    #endregion

    #region Helpers

    private int FindTabIndex(SpellCard card) {
        for (int i = 0; i < OpenTabs.Count; i++) {
            if (OpenTabs[i].Card == card) {
                return i;
            }
        }
        return -1;
    }

    private SpellTabViewModel? FindTab(SpellCard card) {
        foreach (var tab in OpenTabs) {
            if (tab.Card == card) {
                return tab;
            }
        }
        return null;
    }

    #endregion

    #region Commands

    #region Tab Commands

    [ReactiveCommand]
    private void OpenTab(SpellCard? card) {
        if (card is null) return;

        // If it's already open, navigate to the tab
        int lastTemporary = -1;
        for (int i = 0; i < OpenTabs.Count; i++) {
            if (OpenTabs[i].Card.Id == card.Id) {
                SelectedTab = i;
                return;
            }
            if (OpenTabs[i].IsTemporary) {
                lastTemporary = i;
            }
        }

        if (lastTemporary > -1) {
            // If there's a temporary tab, replace it
            OpenTabs[lastTemporary] = new(card, _markdownRenderService);
            SelectedTab = lastTemporary;
        } else {
            // Otherwise, open in new tab
            OpenTabs.Add(new(card, _markdownRenderService));
            SelectedTab = OpenTabs.Count - 1;
        }
    }

    [ReactiveCommand]
    private void MakeTabPermanent(SpellCard? card) {
        if (Deck is null || card is null) return;

        var tab = FindTab(card);
        if (tab is not null) {
            tab.IsTemporary = false;
        }
    }

    [ReactiveCommand]
    private void CloseTab(SpellTabViewModel tab) => OpenTabs.Remove(tab);

    [ReactiveCommand]
    private void CloseCurrentTab() {
        if (SelectedTab >= 0 && SelectedTab < OpenTabs.Count) {
            OpenTabs.RemoveAt(SelectedTab);
        }
    }

    [ReactiveCommand]
    private void CloseAllTabs() {
        OpenTabs.Clear();
    }

    [ReactiveCommand]
    private void GoToPrevTab() {
        if (OpenTabs.Count == 0) return;
        SelectedTab = (OpenTabs.Count + SelectedTab - 1) % OpenTabs.Count;
    }

    [ReactiveCommand]
    private void GoToNextTab() {
        if (OpenTabs.Count == 0) return;
        SelectedTab = SelectedTab + 1 % OpenTabs.Count;
    }

    #endregion

    #region File Commands

    private enum SaveResult { Success, Cancelled, Error }

    /// <summary>
    /// If the current file is dirty, prompt the user to save it.
    /// </summary>
    /// <returns></returns>
    private async Task<SaveResult> AskAndSaveIfDirty() {
        if (!IsDirty) return SaveResult.Success;
        if (_msgBoxService is null) return SaveResult.Success;

        var message = CurFilePath is null
            ? "You have unsaved changes. Would you like to save them?"
            : $"{Path.GetFileName(CurFilePath.LocalPath)} has unsaved changes. Would you like to " +
            "save them?";

        var saveChangesResult = await _msgBoxService.ShowWarningBox(
            "Save", message, MessageBoxButtons.YesNo);
        if (saveChangesResult != MessageBoxResult.Yes) return SaveResult.Success;

        return await SaveImpl(CurFilePath);
    }

    [ReactiveCommand]
    private async Task NewFile() {
        if (await AskAndSaveIfDirty() != SaveResult.Success) return;

        Deck = new CardCollection();
        //OpenTabs.Clear();
        CurFilePath = null;
        IsDirty = false;
    }

    [ReactiveCommand(OutputScheduler = nameof(RxApp.MainThreadScheduler))]
    private async Task OpenFile() {
        if (await AskAndSaveIfDirty() != SaveResult.Success) return;

        try {
            using var file = await _fileService.OpenDeckFile();
            if (file is null) return;

            using var stream = await file.OpenReadAsync();
            CardCollection? collection = CardCollection.FromJson(stream);
            if (collection is not null) {
                Deck = collection;
                //OpenTabs.Clear();
                CurFilePath = file.Path;
                IsDirty = false;
            }
        } catch (Exception ex) {
            await _msgBoxService.ShowErrorBox(
                "Error",
                $"There was an error opening the file:\n{ex.Message}");
        }
    }

    [ReactiveCommand]
    private async Task SaveCurrentFile() => await SaveImpl(CurFilePath);

    [ReactiveCommand]
    private async Task SaveAs() => await SaveImpl();

    /// <summary>
    /// Saves the current file.
    /// </summary>
    /// <param name="filePath">
    /// The URI of the file to save, or <c>null</c> to prompt the user for a path.
    /// </param>
    /// <param name="rethrow">
    /// Whether to rethrow any exceptions, as opposed to showing a message box.
    /// </param>
    /// <returns><c>true</c> if the file was successfully saved, <c>false</c> otherwise.</returns>
    private async Task<SaveResult> SaveImpl(Uri? filePath = null, bool rethrow = false) {
        try {
            if (Deck is null) throw new NullReferenceException();

            using var file = filePath is null
                ? await _fileService.SaveDeckFileAs()
                : await _fileService.TryGetFile(filePath);

            if (file is null) return SaveResult.Cancelled;

            using var stream = await file.OpenWriteAsync();
            if (Path.GetExtension(file.Name) == ".scdeck") {
                Deck.WriteCompressed(stream);
            } else {
                Deck.WriteUncompressed(stream);
            }

            CurFilePath = file.Path;
            IsDirty = false;
            return SaveResult.Success;
        } catch (Exception ex) {
            if (rethrow) throw;

            await _msgBoxService.ShowErrorBox(
                "Error",
                $"There was an error saving the file:\n{ex.Message}");
            return SaveResult.Error;
        }
    }

    #endregion

    #region Search Panel Commands

    [ReactiveCommand]
    private void AddTagToSearch() {
        //var foundTag = Collection.Tags.FirstOrDefault(tag => tag?.Name == TagSearchBoxText, null);
        foreach (var tag in Deck.Tags.Items) {
            if (tag is not null && tag.Name == TagSearchBoxText && !SearchedTags.Contains(tag)) {
                SearchedTags.Add(tag);
                TagSearchBoxText = "";
                break;
            }
        }
    }

    [ReactiveCommand]
    private void RemoveTagFromSearch(string tagName) {
        for (var i = 0; i < SearchedTags.Count; i++) {
            if (SearchedTags[i].Name == tagName) {
                SearchedTags.RemoveAt(i);
                break;
            }
        }
    }

    [ReactiveCommand]
    private void ToggleTagForCard(IList args) {
        if (args.Count != 2
            || args[0] is not Tag tag
            || args[1] is not SpellCard card) return;

        if (!card.Tags.Remove(tag)) {
            card.Tags.Add(tag);
        }
    }

    [ReactiveCommand]
    private void ToggleFavoriteForCard(SpellCard card) {
        card.IsFavorite = !card.IsFavorite;
    }

    #endregion

    #region Editor Commands

    [ReactiveCommand]
    private async Task EditSpell(SpellCard? card) {
        if (card is not null) {
            await Interactions.OpenSpellEditor.Handle(card);
        }
    }

    [ReactiveCommand(CanExecute = nameof(_selectedCardIsNotNull))]
    private async Task EditSelectedSpell() {
        if (SelectedCard is not null) {
            await Interactions.OpenSpellEditor.Handle(SelectedCard);
        }
    }

    [ReactiveCommand]
    private async Task EditNewSpell() => await Interactions.OpenSpellEditor.Handle(null);

    [ReactiveCommand]
    private async Task EditNewSpellWithLevel(string level) {
        var card = new SpellCard { Name = "", Attributes = [new("Level", level)] };
        await Interactions.OpenSpellEditor.Handle(card);
    }

    [ReactiveCommand]
    private async Task EditTags() => await Interactions.OpenTagEditor.Handle(Unit.Default);

    #endregion

    #region Spell List Commands

    [ReactiveCommand]
    private void RemoveCard(SpellCard card) {
        if (!Deck.HasCard(card)) return;

        var tabIndex = FindTabIndex(card);
        if (tabIndex >= 0) {
            OpenTabs.RemoveAt(tabIndex);
        }

        Deck.RemoveCard(card);
    }

    [ReactiveCommand]
    private void DuplicateCard(SpellCard card) {
        var newCard = card.CloneWithNewId();
        Deck.Cards.AddOrUpdate(newCard);
    }

    #endregion

    #endregion
}

#if DEBUG

public class MainViewModelDesign : MainViewModel {
    public MainViewModelDesign() : base(null!, null!, null!) {
        List<Tag> tags = [
            new() { Name = "Combat", Color = Color.Red },
            new() { Name = "Electric", Color = Color.CadetBlue },
        ];
        Deck.Tags.AddOrUpdate(tags);
        Deck.Cards.AddOrUpdate([
            new() {
                Name = "Call Lightning",
                Attributes = [new("Level", "3")],
                Tags = [tags[0]],
            },
            new() {
                Name = "Create Water",
                Attributes = [new("Level", "0")],
                IsFavorite = true,
            },
        ]);

        SelectedCard = Deck.Cards.Items[0];
        SelectedTab = 0;
    }
}
#endif