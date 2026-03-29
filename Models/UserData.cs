using System;
using System.Collections.Generic;

namespace Unbreakfocus.Models
{
    public class UserData
    {
        public string UserName { get; set; } = "Aspirant";
        public string Target { get; set; } = "Custom";
        public DateTime? TargetDate { get; set; }
        public int LifetimeXp { get; set; } = 0;
        public int Streak { get; set; } = 0;
        public DateTime LastDate { get; set; } = DateTime.Now;
        public int DailyGlobalSeconds { get; set; } = 0;
        public bool IsStrictMode { get; set; } = false;
        
        public List<Subject> Subjects { get; set; } = new();

        public List<string> BlockedExes { get; set; } = new() { "discord", "steam", "msedge", "bgmi" };
        public List<string> BlockedKeywords { get; set; } = new() { "discord", "netflix", "instagram", "youtube", "twitter", "reddit" };
    }
}