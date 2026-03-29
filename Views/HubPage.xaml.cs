using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Unbreakfocus.Services;

namespace Unbreakfocus.Views
{
    public sealed partial class HubPage : Page
    {
        public HubPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var user = StorageService.CurrentUser;
            if (user == null) return;

            ProfileNameText.Text = string.IsNullOrEmpty(user.UserName) ? "ASPIRANT" : user.UserName.ToUpper();
            TargetText.Text = string.IsNullOrEmpty(user.Target) ? "TARGET: CUSTOM" : $"TARGET: {user.Target.ToUpper()}";

            // Leveling logic: Powers of 5 based on your Dart code
            int currentLevel = 0;
            int threshold = 5;
            int tempXp = user.LifetimeXp;
            
            if (tempXp >= 5)
            {
                while (tempXp >= threshold)
                {
                    currentLevel++;
                    threshold *= 5;
                }
            }

            LevelText.Text = $"LEVEL {currentLevel}";
            XpText.Text = $"({user.LifetimeXp} XP)";
            StreakText.Text = $"{user.Streak} DAYS";

            TimeSpan ts = TimeSpan.FromSeconds(user.DailyGlobalSeconds);
            TotalTimeText.Text = ts.Hours > 0 ? $"{(int)ts.TotalHours}h {ts.Minutes}m" : $"{ts.Minutes}m";
        }
    }
}