using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class MessageDTO
    {
        [Required]
        public string UserId { get; set; }
        [Required, MaxLength(3000)]
        public string Text { get; set; }
        public bool isRead { get; set; }
        public int? PrivateChatId { get; set; }
        public int? GroupChatId { get; set; }
    }
}
