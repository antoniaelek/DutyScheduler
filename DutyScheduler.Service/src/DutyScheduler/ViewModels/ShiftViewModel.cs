using System;
using Newtonsoft.Json;

namespace DutyScheduler.ViewModels
{
    public class ShiftViewModel
    {
        public string Date { get; set; }
        public string UserId { get; set; }
        public bool IsReplaceable { get; set; }
    }
}
