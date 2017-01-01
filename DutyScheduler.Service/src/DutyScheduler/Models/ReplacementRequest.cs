using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DutyScheduler.Models
{
    public class ReplacementRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public int ShiftId { get; set; }

        public DateTime? Date { get; set; }

        //[ForeignKey("UserId")]
        //public virtual User User { get; set; }

        //[ForeignKey("ShiftId")]
        //public virtual Shift Shift { get; set; }
    }
}
