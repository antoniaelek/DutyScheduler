using DutyScheduler.Models;

namespace DutyScheduler.ViewModels
{
    public class DayViewModel
    {
        public string Date { get; set; }
        public string Weekday { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public bool? IsPrefered { get; set; }
        public bool? IsReplaceable { get; set; }
        public int? ShiftId { get; set; }
        public object Scheduled { get; set; }
    }
}
