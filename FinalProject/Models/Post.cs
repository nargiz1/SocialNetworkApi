using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class Post : Base
    {
        [MaxLength(2000)]
        public string Text { get; set; }
        public bool IsPrivate { get; set; }
        public string Location { get; set; }
        public bool IsStory { get; set; }
        public bool CommentsExist { get; set; }
        public string UserId { get; set; }
        public ApiUser User { get; set; }
        public List<PostImage> Images { get; set; }
        public List<PostVideo> Videos { get; set; }
        public List<PostLike> Likes { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
