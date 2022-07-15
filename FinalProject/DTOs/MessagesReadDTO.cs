using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class MessagesReadDTO
    {
        public string UserId { get; set; }
        public int? PrivateChatId { get; set; }
        public int? GroupChatId { get; set; }
    }
}
