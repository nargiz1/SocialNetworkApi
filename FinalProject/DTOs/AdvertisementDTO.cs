using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class AdvertisementDTO
    {
        public int Id { get; set; }
        [MaxLength(2000)]
        public string Text { get; set; }
        public IFormFile ImageFile { get; set; }
        public IFormFile VideoFile { get; set; }
        public DateTime Deadline { get; set; }
    }
}
