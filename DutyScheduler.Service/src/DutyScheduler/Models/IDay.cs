using System;

namespace DutyScheduler.Models
{
    public interface IDay
    {
        DateTime Date { get; }
        string Name { get; }
        string Type { get; }
        string Scheduled { get; set; }
        bool IsReplaceable { get; set; }
    }
}
