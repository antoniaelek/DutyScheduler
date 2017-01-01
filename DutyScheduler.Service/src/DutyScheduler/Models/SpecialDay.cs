using System;

namespace DutyScheduler.Models
{
    public class SpecialDay : IDay
    {
        public DateTime Date { get; }
        public string Name { get; }
        public string Type { get; } = "special";
        public bool IsReplaceable { get; set; }
        public string Scheduled { get; set; }

        public SpecialDay(string name, DateTime date)
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
