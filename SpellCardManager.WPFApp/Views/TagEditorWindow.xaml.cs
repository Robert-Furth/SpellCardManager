using ReactiveUI;
using ReactiveUI.SourceGenerators;
using SpellCardManager.ViewModels;
using SpellCardManager.WPFApp.Converters;
using System.Windows;

namespace SpellCardManager.WPFApp.Views;

/// <summary>
/// Interaction logic for TagEditorWindow.xaml
/// </summary>
[IViewFor<TagEditorViewModel>]
public partial class TagEditorWindow : Window {
    public TagEditorWindow() {
        InitializeComponent();

        this.WhenActivated(d => {
            ViewModel = (TagEditorViewModel)DataContext;
        });
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e) {
        var converter = (StringOrDefaultConverter)Resources["stringOrDevaultConverter"];
        converter.VisualIsEmpty = false;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e) {
        ViewModel.SaveChangesCommand.Execute().Subscribe();
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();
}
