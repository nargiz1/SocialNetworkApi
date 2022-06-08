using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class CommentComment : Base
    {
        [Required, MaxLength(1000)]
        public string Text { get; set; }
        public string UserId { get; set; }
        public ApiUser User { get; set; }
        public int CommentId { get; set; }
        public PostComment Comment { get; set; }
    }
}
