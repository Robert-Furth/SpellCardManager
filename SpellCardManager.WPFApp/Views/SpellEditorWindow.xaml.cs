using ModernWpf.Controls;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SpellCardManager.ViewModels;
using System.Windows;

namespace SpellCardManager.WPFApp.Views;

/// <summary>
/// Interaction logic for SpellEditorWindow.xaml
/// </summary>
[IViewFor<SpellEditorViewModel>]
public partial class SpellEditorWindow : Window {
    public SpellEditorWindow() {
        InitializeComponent();

        this.WhenActivated(d => {
            ViewModel = (SpellEditorViewModel)DataContext;
        });
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e) {
        ViewModel.SaveChangesCommand.Execute().Subscribe();
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) {
        Close();
    }

    private void AutoSuggestBox_QuerySubmitted(
        AutoSuggestBox sender,
        AutoSuggestBoxQuerySubmittedEventArgs args) {

        ViewModel.AddTagCommand.Execute().Subscribe();
        sender.ItemsSource = new List<string>();
    }

    private void AutoSuggestBox_TextChanged(
        AutoSuggestBox sender,
        AutoSuggestBoxTextChangedEventArgs args) {

        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) {
            sender.ItemsSource = ViewModel.AvailableTags
                .Where(tagName => tagName.StartsWith(
                    sender.Text, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
