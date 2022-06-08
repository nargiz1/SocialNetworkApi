using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class PostLike : Base
    {
        public string UserId { get; set; }
        public ApiUser User { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}
