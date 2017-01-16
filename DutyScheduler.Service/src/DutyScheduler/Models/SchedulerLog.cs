using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DutyScheduler.Models
{
    public class SchedulerLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime DateGenerated { get; set; }

        [Required]
        public string ForMonth { get; set; }
    }
}
