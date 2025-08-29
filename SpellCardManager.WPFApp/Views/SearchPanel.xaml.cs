using DynamicData;
using DynamicData.Binding;
using ModernWpf.Controls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SpellCardManager.ViewModels;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Data;

namespace SpellCardManager.WPFApp.Views;

/// <summary>
/// Interaction logic for SearchPanel.xaml
/// </summary>
/// 

[IViewFor<MainViewModel>]
public partial class SearchPanel {
    public SearchPanel() {
        InitializeComponent();

        this.WhenActivated(d => {
            ViewModel = (MainViewModel)DataContext;

            this.WhenAnyValue(x => x.ViewModel.SortedCards)
                .Do(_ => UpdateSearchResultsCollection())
                .Select(cards => cards.ToObservableChangeSet())
                .Switch()
                .AutoRefresh(card => card.Level)
                //.AutoRefresh()
                .Do(_ => RefreshSearchResultsGrouping())
                .Subscribe()
                .DisposeWith(d);

            this.WhenAnyValue(x => x.ViewModel.GroupByLevel)
                .Subscribe(_ => InitSearchResultsGrouping())
                .DisposeWith(d);
        });
    }

    private void UpdateSearchResultsCollection() {
        var view = (CollectionView)CollectionViewSource.GetDefaultView(SearchResults.ItemsSource);
        var groupDesc = new PropertyGroupDescription("Level");
        view.GroupDescriptions.Add(groupDesc);
    }

    private void RefreshSearchResultsGrouping() { }

    private void InitSearchResultsGrouping() {
        var view = (CollectionView)CollectionViewSource.GetDefaultView(SearchResults.ItemsSource);
        view.GroupDescriptions.Clear();
        if (ViewModel.GroupByLevel) {
            view.GroupDescriptions.Add(new PropertyGroupDescription("Level"));
        }
    }

    private void AutoSuggestBox_QuerySubmitted(
        AutoSuggestBox sender,
        AutoSuggestBoxQuerySubmittedEventArgs args) {

        ViewModel.AddTagToSearchCommand.Execute().Subscribe();
        sender.ItemsSource = new List<string>();
    }

    // Why do I have to do this manually?!
    private void AutoSuggestBox_TextChanged(
        AutoSuggestBox sender,
        AutoSuggestBoxTextChangedEventArgs args) {

        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
            sender.ItemsSource = ViewModel?.TagNames?
                .Where(tagName => tagName.StartsWith(
                    sender.Text, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
