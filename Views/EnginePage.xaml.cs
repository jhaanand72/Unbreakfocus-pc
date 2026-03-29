using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Unbreakfocus.Models;
using Unbreakfocus.Services;

namespace Unbreakfocus.Views
{
    public sealed partial class EnginePage : Page
    {
        private Subject _currentSubject;
        private DispatcherTimer _timer;
        private FocusEngine _focusEngine;

        private int _totalGoalSeconds;
        private int _secondsRemaining;
        private int _overtimeSeconds = 0;
        private bool _isOvertime = false;
        private bool _isPaused = false;
        private int _sessionXpEarned = 0;

        public EnginePage()
        {
            this.InitializeComponent();
            _focusEngine = new FocusEngine();
            _focusEngine.OnDistractionDetected += FocusEngine_OnDistractionDetected;
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string subjectId)
            {
                _currentSubject = StorageService.CurrentUser.Subjects.Find(s => s.Id == subjectId);
                InitializeSession();
            }
        }

        private void InitializeSession()
        {
            if (_currentSubject == null) return;

            SubjectNameText.Text = _currentSubject.Name.ToUpper();
            _totalGoalSeconds = _currentSubject.GoalMins * 60;

            if (_currentSubject.TimeDone >= _totalGoalSeconds)
            {
                _secondsRemaining = 0;
                _overtimeSeconds = _currentSubject.TimeDone - _totalGoalSeconds;
                TriggerOvertimeUI();
            }
            else
            {
                _secondsRemaining = _totalGoalSeconds - _currentSubject.TimeDone;
            }

            UpdateDisplay();
            
            // Start the clocks and the Win32 Tracker!
            _timer.Start();
            _ = _focusEngine.StartWatchdogAsync();
        }

        private void Timer_Tick(object sender, object e)
        {
            if (_isPaused) return;

            // Rebalanced XP Math from your Flutter app
            _currentSubject.XpBuffer += (0.2 / 60.0);
            if (_currentSubject.XpBuffer >= 1.0)
            {
                int earned = (int)_currentSubject.XpBuffer;
                StorageService.CurrentUser.LifetimeXp += earned;
                _sessionXpEarned += earned;
                _currentSubject.XpBuffer -= earned;
            }

            _currentSubject.TimeDone++;
            StorageService.CurrentUser.DailyGlobalSeconds++;

            if (_isOvertime)
            {
                _overtimeSeconds++;
            }
            else
            {
                _secondsRemaining--;
                if (_secondsRemaining <= 0)
                {
                    TriggerOvertimeUI();
                }
            }

            UpdateDisplay();
            
            // Auto-save every 30 seconds
            if (_currentSubject.TimeDone % 30 == 0)
            {
                _ = StorageService.SaveUserAsync();
            }
        }

        private void UpdateDisplay()
        {
            int displaySeconds = _isOvertime ? _overtimeSeconds : _secondsRemaining;
            TimeSpan time = TimeSpan.FromSeconds(displaySeconds);
            
            TimeDisplay.Text = time.Hours > 0 
                ? time.ToString(@"hh\:mm\:ss") 
                : time.ToString(@"mm\:ss");

            if (!_isOvertime && _totalGoalSeconds > 0)
            {
                TimerProgress.Value = ((double)_secondsRemaining / _totalGoalSeconds) * 100;
            }
        }

        private void TriggerOvertimeUI()
        {
            _isOvertime = true;
            TimerProgress.Value = 100;
            TimerProgress.Foreground = (SolidColorBrush)Application.Current.Resources["Emerald"];
            
            OvertimeIndicator.Visibility = Visibility.Visible;
            StatusText.Text = "LOGGED";

            AbortBtn.BorderBrush = (SolidColorBrush)Application.Current.Resources["Emerald"];
            AbortIcon.Glyph = "\xE73E"; // Checkmark
            AbortIcon.Foreground = (SolidColorBrush)Application.Current.Resources["Emerald"];
            AbortText.Text = "FINISH";
            AbortText.Foreground = (SolidColorBrush)Application.Current.Resources["Emerald"];
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StorageService.CurrentUser.IsStrictMode && !_isOvertime) return; // Strict mode block

            _isPaused = !_isPaused;
            PauseIcon.Glyph = _isPaused ? "\xE768" : "\xE769"; // Swap Play/Pause icon
        }

        private async void AbortBtn_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _focusEngine.StopWatchdog();
            await StorageService.SaveUserAsync();

            // Navigate back to Dashboard (Wait, we're in a Frame, let's go back)
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        // Triggers when FocusEngine spots Discord/Chrome/etc.
        // Inside Views/EnginePage.xaml.cs

        private void FocusEngine_OnDistractionDetected(string appName)
        {
            // 1. Pause the timer to prevent cheating
            _isPaused = true;
        
            // 2. Open the full-screen Blocker Window natively using the UI Thread
            DispatcherQueue.TryEnqueue(() =>
            {
                var blocker = new BlockerWindow(appName);
                
                // 3. When the user closes the blocker (Overrides it), resume the timer
                blocker.Closed += (s, args) =>
                {
                    _isPaused = false;
                };
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _timer.Stop();
            _focusEngine.StopWatchdog();
            base.OnNavigatedFrom(e);
        }
    }
}