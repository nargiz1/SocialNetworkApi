using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class GroupChatToUser
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApiUser User { get; set; }
        public int GroupChatId { get; set; }
        public GroupChat GroupChat { get; set; }
    }
}
