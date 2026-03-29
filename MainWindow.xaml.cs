using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Unbreakfocus
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            MainNav.SelectedItem = MainNav.MenuItems[0]; // Auto-select Hub on launch
        }

        private void MainNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var item = args.SelectedItem as NavigationViewItem;
            
            switch (item.Tag)
            {
                case "Hub":
                    ContentFrame.Navigate(typeof(Views.HubPage));
                    break;
                case "Focus":
                    ContentFrame.Navigate(typeof(Views.DashboardPage));
                    break;
                case "Settings":
                    ContentFrame.Navigate(typeof(Views.SettingsPage));
                    break;
                case "Credits":
                    ContentFrame.Navigate(typeof(Views.CreditsPage));
                    break;
            }
        }
    }
}