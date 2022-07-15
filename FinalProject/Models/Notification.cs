using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class Notification: Base
    {
        public string FromUserId { get; set; }
        public ApiUser FromUser { get; set; }
        public string ToUserId { get; set; }
        public ApiUser ToUser { get; set; }
        public string Text { get; set; }
        public bool IsRead { get; set; }
    }
}
