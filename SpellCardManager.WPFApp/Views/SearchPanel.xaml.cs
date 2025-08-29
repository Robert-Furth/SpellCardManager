using DynamicData;
using DynamicData.Binding;
using ModernWpf.Controls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SpellCardManager.Backend.Models;
using SpellCardManager.ViewModels;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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
                .AutoRefresh()
                .Do(_ => ResizeGridViewColumns())
                .Do(_ => RefreshSearchResultsGrouping())
                .Subscribe()
                .DisposeWith(d);

            this.WhenAnyValue(x => x.ViewModel.GroupByLevel)
                .Subscribe(_ => InitSearchResultsGrouping())
                .DisposeWith(d);
        });
    }

    private void UpdateSearchResultsCollection() {
        var view = CollectionViewSource.GetDefaultView(SearchResults.ItemsSource);
        var groupDesc = new PropertyGroupDescription("Level");
        view.GroupDescriptions.Add(groupDesc);
    }

    private void InitSearchResultsGrouping() {
        var view = CollectionViewSource.GetDefaultView(SearchResults.ItemsSource);
        view.GroupDescriptions.Clear();
        if (ViewModel.GroupByLevel) {
            view.GroupDescriptions.Add(new PropertyGroupDescription("Level"));
        }
    }

    private void RefreshSearchResultsGrouping() {
        if (ViewModel.GroupByLevel) {
            var view = CollectionViewSource.GetDefaultView(SearchResults.ItemsSource);
            view.Refresh();
        }
    }

    private static void ResizeGridViewColumn(GridViewColumn col) {
        if (double.IsNaN(col.Width)) {
            col.Width = col.ActualWidth;
        }
        col.Width = double.NaN;
    }

    private void ResizeGridViewColumns() {
        ResizeGridViewColumn(SearchResultCol1);
        ResizeGridViewColumn(SearchResultCol2);
    }

    #region Event Handlers

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

    private void SearchResults_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
        if (e.OriginalSource is not FrameworkElement src) return;

        ContextMenu = new ContextMenu();
        if (src.DataContext is SpellCard card) {
            Debug.WriteLine(card);

            ContextMenu.Items.Add(new MenuItem {
                Header = "Edit Spell...",
                Command = ViewModel.EditSpellCommand,
                CommandParameter = card,
            });

            ContextMenu.Items.Add(new MenuItem {
                Header = "Duplicate Spell",
                Command = ViewModel.DuplicateCardCommand,
                CommandParameter = card,
            });

            ContextMenu.Items.Add(new MenuItem {
                Header = "Delete Spell",
                Command = ViewModel.RemoveCardCommand,
                CommandParameter = card,
            });

            ContextMenu.Items.Add(new Separator());
        }

        ContextMenu.Items.Add(new MenuItem {
            Header = "New Spell...",
            Command = ViewModel.EditNewSpellCommand
        });


    }
    private void SearchResults_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
        if (e.OriginalSource is not FrameworkElement src || src.DataContext is not SpellCard card)
            return;

        ViewModel.MakeTabPermanentCommand.Execute(card).Subscribe();
    }

    #endregion
}
