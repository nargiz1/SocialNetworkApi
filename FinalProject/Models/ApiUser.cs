using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Models
{
    public class ApiUser : IdentityUser
    {
        [Required, MaxLength(1000)]
        public string FullName { get; set; }
        [MaxLength(10)]
        public string BirthDate { get; set; }
        [MaxLength(500)]
        public string RelationshipStatus { get; set; }
        [MaxLength(500)]
        public string Occupation { get; set; }
        [MaxLength(1000)]
        public string Education { get; set; }
        [MaxLength(13)]
        public string Status { get; set; }
        [MaxLength(1000)]
        public string Country { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; }
        public string CoverPicUrl { get; set; }
        public List<SocialMediaLink> SocialMediaLinks { get; set; }
        public List<Post> Posts { get; set; }

    }
}
