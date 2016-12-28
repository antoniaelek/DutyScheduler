using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace DutyScheduler.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public DateTime DateCreated { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public bool IsAdmin { get; set; }
        public string Office { get; set; }
        public string Phone { get; set; }
        //public virtual ICollection<string> Emails { get; set; } 

        public ApplicationUser()
        {
            DateCreated = DateTime.Now;
        }
    }
}
