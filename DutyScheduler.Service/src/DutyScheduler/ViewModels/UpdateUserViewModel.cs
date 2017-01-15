using System.ComponentModel.DataAnnotations;

namespace DutyScheduler.ViewModels
{
    public class UpdateUserViewModel
    {
        public string Name { get; set; }

        public string LastName { get; set; }

        public string Office { get; set; }

        public string Phone { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [MinLength(4)]
        public string Password { get; set; }
    }
}
