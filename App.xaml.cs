using Microsoft.UI.Xaml;
using Unbreakfocus.Services;

namespace Unbreakfocus
{
    public partial class App : Application
    {
        private Window m_window;

        public App()
        {
            this.InitializeComponent();
        }

        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // 1. Load the offline user data first before the UI loads
            await StorageService.LoadUserAsync();

            // 2. Launch the main window (Sidebar)
            m_window = new MainWindow();
            
            // Give the window a native Windows 11 title
            m_window.Title = "Unbreakfocus"; 
            m_window.Activate();
        }
    }
}