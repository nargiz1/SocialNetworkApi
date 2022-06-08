using FinalProject.DAL;
using FinalProject.DTOs;
using FinalProject.Models;
using FinalProject.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApiUser> _userManager;

        public PostController(ApiDbContext db, UserManager<ApiUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }


        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] PostDTO dto)
        {
            if (dto == null) return BadRequest();

            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            ApiUser user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return BadRequest("user not found!");

            Post newPost = new Post()
            {
                Created = DateTime.Now,
                Text = dto.Text,
                IsPrivate = dto.IsPrivate,
                Location = dto.Location,
                UserId = user.Id,
            };
            await _db.Posts.AddAsync(newPost);
            await _db.SaveChangesAsync();

            if(dto.ImageFiles != null)
            {
                foreach(IFormFile item in dto.ImageFiles)
                {
                    if(Extensions.IsImage(item) && Extensions.IsvalidSize(item, 500))
                    {
                        PostImage newPostImage = new PostImage()
                        {
                            ImageUrl = await Extensions.Upload(item, @"files/images"),
                            PostId = newPost.Id,
                        };
                        await _db.PostImages.AddAsync(newPostImage);
                        await _db.SaveChangesAsync();
                    }
                }
            }
            if (dto.VideoFiles != null)
            {
                foreach (IFormFile item in dto.VideoFiles)
                {
                    if (Extensions.IsVideo(item) && Extensions.IsvalidSize(item, 1000))
                    {
                        PostVideo newPostVideo = new PostVideo()
                        {
                            VideoUrl = await Extensions.Upload(item, @"files/videos"),
                            PostId = newPost.Id,
                        };
                        await _db.PostVideos.AddAsync(newPostVideo);
                        await _db.SaveChangesAsync();
                    }
                }
            }
            return Ok("Post added successfully!");
        }
    }
}
