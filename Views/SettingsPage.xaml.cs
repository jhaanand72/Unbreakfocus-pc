using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Unbreakfocus.Services;

namespace Unbreakfocus.Views
{
    public sealed partial class SettingsPage : Page
    {
        private bool _isInitializing = true;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var user = StorageService.CurrentUser;
            if (user == null) return;

            // Load values into UI without triggering save events
            NameInput.Text = user.UserName;
            
            if (TargetDropdown.Items.Contains(user.Target))
            {
                TargetDropdown.SelectedItem = user.Target;
            }
            else
            {
                TargetDropdown.SelectedItem = "Custom";
                CustomTargetInput.Text = user.Target;
                CustomTargetInput.Visibility = Visibility.Visible;
            }

            if (user.TargetDate.HasValue)
            {
                TargetDatePicker.Date = user.TargetDate.Value;
            }

            StrictModeToggle.IsOn = user.IsStrictMode;

            _isInitializing = false;
        }

        private async void Profile_Changed(object sender, object e)
        {
            if (_isInitializing) return;

            var user = StorageService.CurrentUser;
            user.UserName = NameInput.Text.Trim();

            string selected = TargetDropdown.SelectedItem?.ToString();
            if (selected == "Custom")
            {
                CustomTargetInput.Visibility = Visibility.Visible;
                user.Target = CustomTargetInput.Text.Trim();
            }
            else
            {
                CustomTargetInput.Visibility = Visibility.Collapsed;
                user.Target = selected;
            }

            await StorageService.SaveUserAsync();
        }

        private async void TargetDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (_isInitializing) return;

            if (args.NewDate.HasValue)
            {
                StorageService.CurrentUser.TargetDate = args.NewDate.Value.DateTime;
                await StorageService.SaveUserAsync();
            }
        }

        private async void StrictModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;

            var user = StorageService.CurrentUser;

            // Prevent turning off Strict Mode easily if you want to implement the "Wait till midnight" rule
            if (user.IsStrictMode && !StrictModeToggle.IsOn)
            {
                // For now, we allow them to toggle it, but in the future you can block this
            }

            user.IsStrictMode = StrictModeToggle.IsOn;
            await StorageService.SaveUserAsync();
        }
    }
}