using System.ComponentModel.DataAnnotations;

namespace DutyScheduler.Models
{
    public class EmailAccount
    {
        [Key]
        public int Id { get; set; }
        public System.Guid UserId { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        /*
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        */

    }
}
