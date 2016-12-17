using System.ComponentModel.DataAnnotations;

namespace DutyScheduler.ViewModels
{
    public class UpdateUserViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}
