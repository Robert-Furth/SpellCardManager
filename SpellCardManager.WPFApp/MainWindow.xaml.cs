using ReactiveUI.SourceGenerators;
using SpellCardManager.ViewModels;
using SpellCardManager.WPFApp.Services;
using SpellCardManager.WPFApp.Views;
using System.Reactive;
using System.Windows;

namespace SpellCardManager.WPFApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>

[IViewFor<MainViewModel>]
public partial class MainWindow : Window {
    public MainWindow() {
        InitializeComponent();

        ViewModel = new MainViewModel(
            new FileService(),
            new MessageBoxService(),
            new MarkdownRenderService());
        DataContext = ViewModel;

        Interactions.OpenSpellEditor.RegisterHandler(interaction => {
            var existingWindow = App.FindWindowOfType<SpellEditorWindow>();
            if (existingWindow is not null) {
                existingWindow.Activate();
                interaction.SetOutput(Unit.Default);
                return;
            }

            var window = interaction.Input is null
                ? new SpellEditorWindow {
                    Title = "New Spell",
                    Icon = Icon,
                    DataContext = ViewModel.CreateSpellEditorVM(),
                }
                : new SpellEditorWindow {
                    Title = "Edit Spell",
                    Icon = Icon,
                    DataContext = ViewModel.CreateSpellEditorVM(interaction.Input),
                };

            window.ShowDialog();
            interaction.SetOutput(Unit.Default);
        });

        Interactions.OpenTagEditor.RegisterHandler(interaction => {
            var existingWindow = App.FindWindowOfType<TagEditorWindow>();
            if (existingWindow is not null) {
                existingWindow.Activate();
                interaction.SetOutput(Unit.Default);
                return;
            }

            var window = new TagEditorWindow {
                Icon = Icon,
                DataContext = ViewModel.CreateTagEditorVM(),
            };

            window.ShowDialog();
            interaction.SetOutput(Unit.Default);
        });

        Closing += async (sender, e) => {
            var canClose = await ViewModel.CheckCanClose();
            e.Cancel = !canClose;
        };
    }

}