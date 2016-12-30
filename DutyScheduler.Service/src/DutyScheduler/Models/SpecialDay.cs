using System;

namespace DutyScheduler.Models
{
    public class SpecialDay : IDay
    {
        public DateTime Date { get; }
        public string Name { get; }

        public SpecialDay(string name, DateTime date)
        {
            Date = date;
            Name = name;
        }
    }
}
