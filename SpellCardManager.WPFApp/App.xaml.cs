using System.Windows;

namespace SpellCardManager.WPFApp;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application {

    public static Window? FindWindowOfType<T>() where T : Window =>
        Current.Windows.OfType<T>().FirstOrDefault();
}

