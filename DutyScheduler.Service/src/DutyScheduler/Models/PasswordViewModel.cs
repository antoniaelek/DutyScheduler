using System.ComponentModel.DataAnnotations;

namespace DutyScheduler.Models
{
    
    public class PasswordViewModel
    {
        [MinLength(4)]
        public string Password { get; set; }
    }
}
