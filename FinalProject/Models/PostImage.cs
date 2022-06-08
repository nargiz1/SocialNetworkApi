using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class PostImage : Base
    {
        public string ImageUrl { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}
