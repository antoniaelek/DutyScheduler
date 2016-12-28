using System.ComponentModel.DataAnnotations;

namespace DutyScheduler.ViewModels
{
    public class UpdateUserViewModel
    {
        //[Required]
        public string Name { get; set; }

        //[Required]
        public string LastName { get; set; }

        //[Required]
        public string Office { get; set; }

        //[Required]
        public string Phone { get; set; }
    }
}
