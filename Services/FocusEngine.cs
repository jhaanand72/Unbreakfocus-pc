using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Unbreakfocus.Services
{
    public class FocusEngine
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public bool IsSessionActive { get; set; } = false;
        public event Action<string> OnDistractionDetected;

        // Browsers to monitor for tab titles
        private readonly string[] _browsers = { "chrome", "msedge", "firefox", "brave", "opera" };

        public async Task StartWatchdogAsync()
        {
            IsSessionActive = true;
            
            while (IsSessionActive)
            {
                CheckForDistractions();
                await Task.Delay(2000); // Poll every 2 seconds
            }
        }

        public void StopWatchdog()
        {
            IsSessionActive = false;
        }

        private void CheckForDistractions()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero) return;

                // 1. Get Process Name
                GetWindowThreadProcessId(hwnd, out uint pid);
                Process p = Process.GetProcessById((int)pid);
                string activeProcess = p.ProcessName.ToLower();

                // 2. Get Window Title (The Browser Tab Name)
                StringBuilder titleBuilder = new StringBuilder(256);
                GetWindowText(hwnd, titleBuilder, 256);
                string windowTitle = titleBuilder.ToString().ToLower();

                var user = StorageService.CurrentUser;
                if (user == null) return;

                // Check A: Is the raw application blocked? (e.g., discord.exe)
                if (user.BlockedExes.Contains(activeProcess) && !_browsers.Contains(activeProcess))
                {
                    OnDistractionDetected?.Invoke(activeProcess.ToUpper());
                    return;
                }

                // Check B: If it's a browser, is the tab title forbidden? (e.g., "Netflix - Google Chrome")
                if (_browsers.Contains(activeProcess))
                {
                    foreach (var keyword in user.BlockedKeywords)
                    {
                        if (windowTitle.Contains(keyword.ToLower()))
                        {
                            OnDistractionDetected?.Invoke(keyword.ToUpper() + " (WEB)");
                            return;
                        }
                    }
                }
            }
            catch
            {
                // Safely ignore system processes or access denied errors
            }
        }
    }
}