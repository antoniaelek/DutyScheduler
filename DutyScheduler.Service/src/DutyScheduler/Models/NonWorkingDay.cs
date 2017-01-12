using System;

namespace DutyScheduler.Models
{
    public class NonWorkingDay : Day
    {
        public override DateTime Date { get; set; }

        public override string Name { get; }

        public override string Type { get; } = "non-working";
        
        public NonWorkingDay(DateTime date)
        {
            Date = date;
            Name = null;
        }
    }
}
