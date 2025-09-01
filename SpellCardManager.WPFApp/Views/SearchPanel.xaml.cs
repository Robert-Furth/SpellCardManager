using DynamicData;
using DynamicData.Binding;
using ModernWpf;
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
    private readonly Dictionary<string, bool> isExpanded = [];

    public SearchPanel() {
        InitializeComponent();

        this.WhenActivated(d => {
            ViewModel = (MainViewModel)DataContext;

            this.WhenAnyValue(x => x.ViewModel.SortedCards)
                .Do(_ => UpdateSearchResultsCollection())
                .Select(cards => cards.ToObservableChangeSet())
                .Switch()
                .Do(changeSet => {
                    // TODO Revisit logic for clearing isExpanded
                    foreach (var change in changeSet) {
                        if (change.Reason == ListChangeReason.Clear) {
                            isExpanded.Clear();
                        }
                    }
                })
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
        var menu = SearchResults.ContextMenu;
        menu.Items.Clear();

        if (e.OriginalSource is FrameworkElement src) {
            if (src.DataContext is SpellCard card) {
                Debug.WriteLine(card);

                menu.Items.Add(new MenuItem {
                    Header = "Edit Spell...",
                    Command = ViewModel.EditSpellCommand,
                    CommandParameter = card,
                });

                var tagItem = new MenuItem { Header = "Set Tags" };

                foreach (var tag in ViewModel.SortedTags) {
                    var subItem = new MenuItem {
                        Header = tag.Name,
                        IsCheckable = true,
                        IsChecked = card.Tags.Contains(tag),
                        Command = ViewModel.ToggleTagForCardCommand,
                        CommandParameter = new List<object> { tag, card },
                        StaysOpenOnClick = true,
                    };
                    subItem.Click += (s, e) => ResizeGridViewColumns();
                    tagItem.Items.Add(subItem);
                }

                menu.Items.Add(tagItem);

                menu.Items.Add(new MenuItem {
                    Header = "Duplicate Spell",
                    Command = ViewModel.DuplicateCardCommand,
                    CommandParameter = card,
                });

                menu.Items.Add(new MenuItem {
                    Header = "Delete Spell",
                    Command = ViewModel.RemoveCardCommand,
                    CommandParameter = card,
                });

                menu.Items.Add(new Separator());
            } else if (src.FindAscendantByName("SearchResultGroup") is FrameworkElement el) {
                // DataContext for ContainerStyle is a private class, so I'll
                // have to use reflection. Ugly hack, but it works.
                object? maybeLevel = el.DataContext
                    .GetType()
                    .GetProperty("Name")?
                    .GetValue(el.DataContext);

                if (maybeLevel is string level && level != "[none]") {
                    menu.Items.Add(new MenuItem {
                        Header = "New Spell at This Level...",
                        Command = ViewModel.EditNewSpellWithLevelCommand,
                        CommandParameter = level,
                    });

                    menu.Items.Add(new Separator());
                }

            }

            menu.Items.Add(new MenuItem {
                Header = "New Spell...",
                Command = ViewModel.EditNewSpellCommand
            });
        }


    }
    private void SearchResults_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
        if (e.OriginalSource is not FrameworkElement src || src.DataContext is not SpellCard card)
            return;

        ViewModel.MakeTabPermanentCommand.Execute(card).Subscribe();
    }

    private void Expander_Expanded(object sender, RoutedEventArgs e) {
        if (sender is not Expander exp || exp.Header is not TextBlock tb) return;
        isExpanded[tb.Text] = true;
    }

    private void Expander_Collapsed(object sender, RoutedEventArgs e) {
        if (sender is not Expander exp || exp.Header is not TextBlock tb) return;
        isExpanded[tb.Text] = false;
    }

    private void Groups_Loaded(object sender, RoutedEventArgs e) {
        if (sender is not DependencyObject dp) return;

        var expanders = dp.FindDescendants<Expander>();
        foreach (var expander in expanders) {
            if (expander.Header is not TextBlock tb) continue;
            expander.IsExpanded = isExpanded.GetValueOrDefault(tb.Text, true);
        }
    }

    #endregion
}
