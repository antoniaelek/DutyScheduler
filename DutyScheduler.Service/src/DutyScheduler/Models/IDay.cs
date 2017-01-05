using System;

namespace DutyScheduler.Models
{
    public interface IDay
    {
        DateTime Date { get; }
        string Name { get; }
        string Type { get; }
        bool IsReplaceable { get; set; }
        bool? IsPrefered { get; set; }
        string Scheduled { get; set; }
    }
}
