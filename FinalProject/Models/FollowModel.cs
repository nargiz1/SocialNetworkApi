using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class FollowModel
    {
        public int Id { get; set; }
        public string FollowingUserId { get; set; }
        public ApiUser FollowingUser { get; set; }
        public string FollowedUserId { get; set; }
        public ApiUser FollowedUser { get; set; }

    }
}
