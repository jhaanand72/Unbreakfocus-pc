using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Unbreakfocus.Views
{
    public sealed partial class BlockerWindow : Window
    {
        public BlockerWindow(string appName)
        {
            this.InitializeComponent();
            
            // Set the app name in the UI
            BlockedAppNameText.Text = appName;

            // Force Full Screen and hide the title bar
            this.ExtendsContentIntoTitleBar = true;
            this.AppWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
            this.Activate();
        }

        private void OverrideBtn_Click(object sender, RoutedEventArgs e)
        {
            // Close the blocker window
            this.Close();
        }
    }
}