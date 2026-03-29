using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Unbreakfocus.Models;

namespace Unbreakfocus.Services
{
    public class StorageService
    {
        // Saves to: C:\Users\Username\AppData\Local\Unbreakfocus\app_data.json
        private static readonly string FolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Unbreakfocus");
        private static readonly string FilePath = Path.Combine(FolderPath, "app_data.json");

        public static UserData CurrentUser { get; set; }

        public static async Task LoadUserAsync()
        {
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }

            if (File.Exists(FilePath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(FilePath);
                    CurrentUser = JsonSerializer.Deserialize<UserData>(json) ?? CreateBlankUser();
                }
                catch
                {
                    // If file is corrupted, create a new user
                    CurrentUser = CreateBlankUser();
                }
            }
            else
            {
                CurrentUser = CreateBlankUser();
            }
        }

        public static async Task SaveUserAsync()
        {
            if (CurrentUser == null) return;

            string json = JsonSerializer.Serialize(CurrentUser, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(FilePath, json);
        }

        private static UserData CreateBlankUser()
        {
            return new UserData();
        }
    }
}