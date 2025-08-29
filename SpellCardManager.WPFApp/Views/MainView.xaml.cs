using SpellCardManager.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpellCardManager.WPFApp.Views {
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl {
        public MainView() {
            InitializeComponent();
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount != 2) return;

            var panel = (DockPanel)sender;
            var vm = (SpellTabViewModel)panel.DataContext;
            vm.IsTemporary = false;
        }
    }
}
