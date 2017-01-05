using System;

namespace DutyScheduler.Models
{
    public class Holiday : IDay
    {
        public DateTime Date { get; }
        public string Name { get; }
        public string Type { get; } = "holiday";
        public bool IsReplaceable { get; set; }
        public bool? IsPrefered { get; set; }
        public string Scheduled { get; set; }

        public Holiday(string name, DateTime date)
        {
            Date = date;
            Name = name;
        }

        public override string ToString()
        {
            return Date.ToString("d.M.yyyy") + " " + Name;
        }
    }
}
