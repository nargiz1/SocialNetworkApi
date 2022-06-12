using FinalProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class PostUpdateDTO
    {
        public int Id { get; set; }
        [Required, MaxLength(2000)]
        public string Text { get; set; }
        public bool IsPrivate { get; set; }
        public string Location { get; set; }
    }
}
