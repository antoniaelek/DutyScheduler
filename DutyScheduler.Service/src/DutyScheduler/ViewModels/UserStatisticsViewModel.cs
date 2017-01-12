using System.Collections.Generic;
using DutyScheduler.Models;

namespace DutyScheduler.ViewModels
{
    public class UserStatisticsViewModel
    {
        public IEnumerable<Shift> Shifts { get; set; }
        public IEnumerable<ReplacementHistory> Replaced { get; set; }
        public IEnumerable<ReplacementHistory> Replacing { get; set; }
    }
}
