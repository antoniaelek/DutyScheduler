﻿using System.ComponentModel.DataAnnotations;

namespace DutyScheduler.ViewModels
{
    public class LoginViewModel
    {
        
        [EmailAddress]
        public string Email { get; set; }
        
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
