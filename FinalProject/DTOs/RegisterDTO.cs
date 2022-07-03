using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class RegisterDTO
    {
        [Required, MaxLength(1000)]
        public string FullName { get; set; }
        public DateTime BirthDate { get; set; }
        [Required, MaxLength(1000)]
        public string UserName { get; set; }
        [Required, EmailAddress, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords does not match!")]
        public string PasswordConfirm { get; set; }
    }
}
