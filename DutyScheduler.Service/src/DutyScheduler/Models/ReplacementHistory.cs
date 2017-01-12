using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DutyScheduler.Models
{
    public class ReplacementHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string ReplacedUserId { get; set; }

        [Required]
        public string ReplacingUserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public DateTime? DateCreated { get; set; }

        [ForeignKey("ReplacedUserId")]
        public virtual User ReplacedUser { get; set; }

        [ForeignKey("ReplacingUserId")]
        public virtual User ReplacingUser { get; set; }
    }
}
