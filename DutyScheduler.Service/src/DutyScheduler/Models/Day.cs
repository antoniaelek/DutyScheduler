using System;

namespace DutyScheduler.Models
{
    public abstract class Day
    {
        private static readonly string DateFormat = "yyyy-MM-dd";

        public abstract DateTime Date { get; set; }

        public abstract string Name { get; }

        public abstract string Type { get; }

        public string WeekDay => Date.DayOfWeek.ToString();

        public bool IsReplaceable { get; set; }

        public bool? IsPrefered { get; set; }

        public string Scheduled { get; set; }

        public override string ToString()
        {
            return Date.ToString(DateFormat);
        }
    }
}
