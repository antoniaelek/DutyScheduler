namespace DutyScheduler.ViewModels
{
    public class DayViewModel
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Scheduled { get; set; }
        public bool IsReplaceable { get; set; }
    }
}
