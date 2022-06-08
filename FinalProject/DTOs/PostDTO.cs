using FinalProject.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class PostDTO
    {
        [Required, MaxLength(2000)]
        public string Text { get; set; }
        public bool IsPrivate { get; set; }
        public string Location { get; set; }
        public List<IFormFile> ImageFiles { get; set; }
        public List<IFormFile> VideoFiles { get; set; }
    }
}
