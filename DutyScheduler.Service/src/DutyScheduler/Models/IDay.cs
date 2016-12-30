using System;

namespace DutyScheduler.Models
{
    public interface IDay
    {
        DateTime Date { get; }
        string Name { get; }
    }
}
