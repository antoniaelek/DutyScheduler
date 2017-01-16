using System;

namespace DutyScheduler.Models
{
    public class SpecialDay : Day
    {
        public override DateTime Date { get; set; }

        public override string Name { get; }

        public override string Type { get; } = "special";
        
        public SpecialDay(string name, DateTime date)
        {
            Date = date;
            Name = name;
        }
    }
}
