using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;

namespace DutyScheduler.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class User : IdentityUser
    {
        //public DateTime DateCreated { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public bool IsAdmin { get; set; }
        public string Office { get; set; }
        public string Phone { get; set; }

        //public virtual ICollection<ReplacementHistory> ReplacedIn { get; set; }
        //public virtual ICollection<ReplacementHistory> ReplacingIn { get; set; }
        //public virtual ICollection<Preference> Preferences { get; set; }

        //public User()
        //{
        //    DateCreated = DateTime.Now;
        //}
    }
}
