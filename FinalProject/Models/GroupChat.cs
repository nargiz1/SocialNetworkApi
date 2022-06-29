using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class GroupChat : Base
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public List<GroupChatToUser> Users { get; set; }
        public List<Message> Messages { get; set; }
    }
}
