using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Unbreakfocus.Models;
using Unbreakfocus.Services;

namespace Unbreakfocus.Views
{
    public sealed partial class DashboardPage : Page
    {
        // ObservableCollection automatically updates the ListView when subjects are added/removed
        private ObservableCollection<Subject> _subjects = new();
        private bool _isDeleteMode = false;

        public DashboardPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            var user = StorageService.CurrentUser;
            if (user == null) return;

            // 1. Update Header
            GreetingText.Text = $"Hi, {user.UserName.ToUpper()}";
            MissionText.Text = $"MISSION: {user.Target.ToUpper()}";

            if (user.TargetDate.HasValue)
            {
                int daysLeft = Math.Max(0, (user.TargetDate.Value - DateTime.Now).Days);
                DaysLeftText.Text = $"{daysLeft} DAYS TO TARGET";
            }
            else
            {
                DaysLeftText.Text = "NO TARGET DATE SET";
            }

            // 2. Load Subjects into the list
            _subjects.Clear();
            foreach (var sub in user.Subjects)
            {
                _subjects.Add(sub);
            }
            SubjectListView.ItemsSource = _subjects;

            // Toggle Empty State
            EmptyStatePanel.Visibility = _subjects.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            // 3. Calculate Global Stats
            int totalGoalSeconds = user.Subjects.Sum(s => s.GoalMins * 60);
            int effectiveSeconds = user.Subjects.Sum(s => Math.Min(s.TimeDone, s.GoalMins * 60));

            double progress = totalGoalSeconds == 0 ? 0 : ((double)effectiveSeconds / totalGoalSeconds) * 100;
            
            // Format Time (e.g. 1h 30m)
            TimeSpan ts = TimeSpan.FromSeconds(user.DailyGlobalSeconds);
            TodayTimeText.Text = ts.Hours > 0 
                ? $"{ts.Hours}h {ts.Minutes}m {ts.Seconds}s TODAY" 
                : $"{ts.Minutes}m {ts.Seconds}s TODAY";

            DailyProgressBar.Value = progress;
            PercentCompleteText.Text = $"{(int)progress}% COMPLETE";

            // Save state to disk just in case
            _ = StorageService.SaveUserAsync();
        }

        private async void AddSubject_Click(object sender, RoutedEventArgs e)
        {
            // Reset inputs
            NewSubjectNameInput.Text = "";
            NewSubjectGoalInput.Value = 60;

            await AddSubjectDialog.ShowAsync();
        }

        private void AddSubjectDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string name = NewSubjectNameInput.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                args.Cancel = true; // Keep dialog open if invalid
                return;
            }

            int goalMins = (int)NewSubjectGoalInput.Value;

            var newSub = new Subject
            {
                Name = name,
                GoalMins = goalMins
            };

            StorageService.CurrentUser.Subjects.Add(newSub);
            RefreshUI();
        }

        private void DeleteModeBtn_Click(object sender, RoutedEventArgs e)
        {
            _isDeleteMode = !_isDeleteMode;
            // Provide visual feedback for delete mode active
            DeleteModeBtn.Foreground = _isDeleteMode ? new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red) : new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White);
            
            // Note: In a full MVVM setup, we would bind the visibility of the delete buttons in the DataTemplate.
            // For this simpler code-behind approach, we toggle a flag. The user has to know to click the item or we implement an ItemContainerStyle.
            // For now, let's keep it simple: we will ask for confirmation if they click an item while in delete mode.
        }

        private void DeleteSubject_Click(object sender, RoutedEventArgs e)
        {
            // Attached to the inner delete button (if made visible)
            var btn = sender as Button;
            string id = btn?.Tag?.ToString();
            
            var subjectToRemove = StorageService.CurrentUser.Subjects.FirstOrDefault(s => s.Id == id);
            if (subjectToRemove != null)
            {
                StorageService.CurrentUser.Subjects.Remove(subjectToRemove);
                RefreshUI();
            }
        }

        private void StartTimer_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string subjectId = btn?.Tag?.ToString();
        
            var selectedSubject = StorageService.CurrentUser.Subjects.FirstOrDefault(s => s.Id == subjectId);
            
            if (selectedSubject != null)
            {
                // Navigate to EnginePage and pass the ID
                Frame.Navigate(typeof(Views.EnginePage), selectedSubject.Id);
            }
        }
    }
}