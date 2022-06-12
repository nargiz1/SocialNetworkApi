using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class CommentLike : Base
    {
        public string UserId { get; set; }
        public ApiUser User { get; set; }
        public int CommentId { get; set; }
        public Comment Comment { get; set; }
    }
}
