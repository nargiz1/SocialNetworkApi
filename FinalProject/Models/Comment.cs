using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class Comment : Base
    {
        [Required, MaxLength(1000)]
        public string Text { get; set; }
        public string UserId { get; set; }
        public ApiUser User { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
        public int? CommentId { get; set; }
        public Comment ReplyComment { get; set; }
        public List<Comment> Comments { get; set; }
        public List<CommentLike> Likes { get; set; }
    }
}
