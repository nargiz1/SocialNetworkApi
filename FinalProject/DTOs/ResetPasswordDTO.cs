using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class ResetPasswordDTO
    {
        [Required, EmailAddress, DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required, DataType(DataType.Password)]
        public string PasswordConfirm { get; set; }
        public string Token { get; set; }
    }
}
