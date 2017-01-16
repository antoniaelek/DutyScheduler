using System;

namespace DutyScheduler.Models
{
    public class OrdinaryDay : Day
    {
        public override DateTime Date { get; set; }

        public override string Name { get; }

        public override string Type { get; } = "ordinary";

        public OrdinaryDay(DateTime date)
        {
            Date = date;
            Name = null;
        }
    }
}
