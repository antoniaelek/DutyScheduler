using System;

namespace DutyScheduler.Models
{
    public class Day : IDay
    {
        public DateTime Date { get; }
        public string Name { get; }

        public Day(DateTime date)
        {
            Date = date;
            Name = string.Empty;
        }
    }
}
