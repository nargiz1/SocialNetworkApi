﻿using FinalProject.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DTOs
{
    public class UpdateDTO
    {
        public string RelationshipStatus { get; set; }
        [MaxLength(500)]
        public string Occupation { get; set; }
        [MaxLength(1000)]
        public string Education { get; set; }
        [MaxLength(13)]
        public string Status { get; set; }
        [MaxLength(1000)]
        public string Country { get; set; }
        public List<string> SocialMediaLinks { get; set; }
        public IFormFile ImageFile { get; set; }
        public IFormFile CoverPicFile { get; set; }
    }
}
