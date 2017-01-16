using System;

namespace DutyScheduler.Models
{
    public class Holiday : Day
    {
        public override DateTime Date { get; set; }

        public override string Name { get; }

        public override string Type { get; } = "holiday";

        public Holiday(string name, DateTime date)
        {
            Date = date;
            Name = name;
        }
    }
}
