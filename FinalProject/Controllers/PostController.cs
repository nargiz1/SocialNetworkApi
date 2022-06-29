using FinalProject.DAL;
using FinalProject.DTOs;
using FinalProject.Models;
using FinalProject.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
            if (dto.Text == null && dto.ImageFiles == null && dto.VideoFiles == null) return BadRequest("Post cannot be empty!");
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            ApiUser user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return BadRequest("user not found!");
            if (dto.PublicationTime < DateTime.Now) dto.PublicationTime = DateTime.Now;
            await Task.Delay((int)(dto.PublicationTime).Subtract(DateTime.Now).TotalMilliseconds);
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
                            ImageUrl = await Extensions.Upload(item, @"files\images"),
                            PostId = newPost.Id,
                            Created = DateTime.Now

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
                            VideoUrl = await Extensions.Upload(item, @"files\videos"),
                            PostId = newPost.Id,
                            Created = DateTime.Now

                        };
                        await _db.PostVideos.AddAsync(newPostVideo);
                        await _db.SaveChangesAsync();
                    }
                }
            }
            return Ok("Post added successfully!");
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] int postToDeleteId)
        {
            Post postToDelete = await _db.Posts.Include(x => x.Images).Include(x=> x.Videos).FirstOrDefaultAsync(x => x.Id == postToDeleteId);
            if (postToDelete == null) return BadRequest("Post not found");
            if (postToDelete.Images != null)
            {
                foreach(PostImage item in postToDelete.Images)
                {
                    string filePath = Path.Combine(@"Files", @"images", item.ImageUrl);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }
            if (postToDelete.Videos != null)
            {
                foreach (PostVideo item in postToDelete.Videos)
                {
                    string filePath = Path.Combine(@"Files", @"videos", item.VideoUrl);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }


            _db.Posts.Remove(postToDelete);
            await _db.SaveChangesAsync();
            return Ok("Post deleted successfully!");
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] PostUpdateDTO updatedPost)
        {
            Post postToUpdate = await _db.Posts.FirstOrDefaultAsync(x => x.Id == updatedPost.Id);
            if (postToUpdate == null) return NotFound("Post not found");
            if (postToUpdate.Images == null && postToUpdate.Videos == null && updatedPost.Text == null) return BadRequest("Post cannot be empty!");
            postToUpdate.Text = updatedPost.Text;
            postToUpdate.Location = updatedPost.Location;
            postToUpdate.IsPrivate = updatedPost.IsPrivate;
            postToUpdate.Updated = DateTime.Now;
            _db.Update(postToUpdate);
            await _db.SaveChangesAsync();
            return Ok(postToUpdate);
        }
        [HttpGet("getPost")]
        public async Task<IActionResult> GetPost([FromBody] int postId)
        {
            Post post = await _db.Posts.Include(x => x.Images)
                .Include(x => x.Videos)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .ThenInclude(x=> x.Comments)
                .ThenInclude(x=> x.Likes).FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null) return NotFound();
            return Ok(post);
        }
        [HttpGet("getUserPosts")]
        public async Task<IActionResult> GetUserPosts([FromBody] string userId)
        {
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            ApiUser user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null) return NotFound();
            List<Post> posts = await _db.Posts
                .Where(x=> x.UserId == userId).ToListAsync();
            return Ok(posts);
        }
        [HttpGet("getAllPosts")]
        public async Task<IActionResult> GetAllPosts([FromQuery] PaginationDTO dto)
        {
            int currentSkip = dto.Skip ?? 1;
            int currentTake = dto.Take ?? 5;
            List<Post> posts = await _db.Posts
                .Include(x=> x.User)
                .Include(x => x.Videos)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .ThenInclude(x => x.Comments)
                .ThenInclude(x => x.Likes).ToListAsync();
            return Ok(posts.Skip(currentSkip).Take(currentTake));
        }
    }
}
