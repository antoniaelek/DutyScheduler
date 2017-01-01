using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DutyScheduler.Models
{
    public class Shift
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public bool IsRepleceable { get; set; }

        //[ForeignKey("UserId")]
        //public virtual User User { get; set; }
    }
}
