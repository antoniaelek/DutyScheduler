using System;

namespace DutyScheduler.Models
{
    public class Holiday : IDay
    {
        public DateTime Date { get; }
        public string Name { get; }

        public Holiday(string name, DateTime date)
        {
            Date = date;
            Name = name;
        }
    }
}
