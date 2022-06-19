using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class Message : Base
    {
        [Required, MaxLength(3000)]
        public string Text { get; set; }
        public bool isRead { get; set; }
        public string UserId { get; set; }
        public ApiUser User { get; set; }
    }
}
