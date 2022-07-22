using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class Story: Base
    {
        public string UserId { get; set; }
        public ApiUser User { get; set; }
        public string ImageUrl { get; set; }
        public string VideoUrl { get; set; }
    }
}
