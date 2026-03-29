using System;

namespace Unbreakfocus.Models
{
    public class Subject
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public int GoalMins { get; set; } = 60;
        public int TimeDone { get; set; } = 0;
        public string ColorHex { get; set; } = "#38BDF8"; // The Unbreakfocus Sky Blue
        public double XpBuffer { get; set; } = 0.0;
    }
}