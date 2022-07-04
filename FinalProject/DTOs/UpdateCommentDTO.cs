using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class UpdateCommentDTO
    {
        public int Id { get; set; }
        [Required, MaxLength(1000)]
        public string Text { get; set; }
    }
}
