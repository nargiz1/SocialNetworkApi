using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class StoryDTO
    {
        public IFormFile ImageFile { get; set; }
        public IFormFile VideoFile { get; set; }
    }
}
