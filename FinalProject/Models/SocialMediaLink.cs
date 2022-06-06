using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class SocialMediaLink
    {
        public int Id { get; set; }
        public string Link { get; set; }
        public string UserId { get; set; }
        public ApiUser User { get; set; }
    }
}
