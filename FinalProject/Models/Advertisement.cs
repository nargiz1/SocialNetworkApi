using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class Advertisement : Base
    {
        [MaxLength(2000)]
        public string Text { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsExpired { get; set; }
        public DateTime Deadline { get; set; }
    }
}
