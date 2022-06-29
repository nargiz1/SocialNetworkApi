using FinalProject.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class GroupChatDTO
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public IFormFile ImageFile { get; set; }
        public List<string> UserIds { get; set; }
    }
}
