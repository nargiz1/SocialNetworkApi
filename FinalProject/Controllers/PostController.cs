using FinalProject.DAL;
using FinalProject.DTOs;
using FinalProject.Models;
using FinalProject.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                IsStory = dto.IsStory
            };
            await _db.Posts.AddAsync(newPost);
            await _db.SaveChangesAsync();

            if (dto.ImageFiles != null)
            {
                foreach (IFormFile item in dto.ImageFiles)
                {
                    if (Files.IsImage(item) && Files.IsvalidSize(item, 500))
                    {
                        PostImage newPostImage = new PostImage()
                        {
                            ImageUrl = Files.Upload(item, "Images"),
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
                    if (Files.IsVideo(item) && Files.IsvalidSize(item, 6000))
                    {
                        PostVideo newPostVideo = new PostVideo()
                        {
                            VideoUrl = Files.Upload(item, "Videos"),
                            PostId = newPost.Id,
                            Created = DateTime.Now

                        };
                        await _db.PostVideos.AddAsync(newPostVideo);
                        await _db.SaveChangesAsync();
                    }
                }
            }
            //if (newPost.IsStory == true) await ExpireStory(newPost.Id);
            return Ok("Post added successfully!");
        }

        [HttpPost("story")]
        public async Task<IActionResult> ExpireStory([FromBody] int postId)
        {
            Post story = await _db.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (story == null) return NotFound("Story not found!");
            await Task.Delay((int)(DateTime.Now.AddSeconds(70)).Subtract(DateTime.Now).TotalMilliseconds);
            await Delete(story.Id);
            return Ok();
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] int postToDeleteId)
        {
            Post postToDelete = await _db.Posts.Include(x => x.Images).Include(x => x.Videos).FirstOrDefaultAsync(x => x.Id == postToDeleteId);
            if (postToDelete == null) return BadRequest("Post not found");
            if (postToDelete.Images != null)
            {
                foreach (PostImage item in postToDelete.Images)
                {
                    Files.Delete(@"Resources", @"Images", item.ImageUrl);
                }
            }
            if (postToDelete.Videos != null)
            {
                foreach (PostVideo item in postToDelete.Videos)
                {
                    Files.Delete(@"Resources", @"Videos", item.VideoUrl);
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
        public async Task<IActionResult> GetPost([FromBody] int Id)
        {
            var post = await _db.Posts
                .Include(x => x.User)
                .Include(x => x.Images)
                .Include(x => x.Videos)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .ThenInclude(x => x.Comments)
                .ThenInclude(x => x.Likes)
                .FirstOrDefaultAsync(x => x.Id == Id);
            if (post == null) return NotFound();
            post.Images.ForEach(x => x.ImageUrl = @"Resources\Images\" + x.ImageUrl);
            post.Videos.ForEach(x => x.VideoUrl = @"Resources\Videos\" + x.VideoUrl);
            return Ok(post);
        }
        [HttpGet("getUserPosts")]
        public async Task<IActionResult> GetUserPosts([FromBody] string userId)
        {
            int currentSkip =  0;
            int currentTake =  5;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();
            List<Post> posts = await _db.Posts
                .Include(x => x.User)
                .Include(x => x.Images)
                .Include(x => x.Videos)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .ThenInclude(x => x.Comments)
                .ThenInclude(x => x.Likes)
                .Where(x => x.UserId == userId).OrderByDescending(x => x.Created).Skip(currentSkip).Take(currentTake).ToListAsync();
            foreach(var item in posts)
            {
                if (!item.User.ImageUrl.Contains(@"Resources\Images\"))
                {
                    item.User.ImageUrl = @"Resources\Images\" + item.User.ImageUrl;
                }
                item.Images.ForEach(x => x.ImageUrl = @"Resources\Images\" + x.ImageUrl);
                item.Videos.ForEach(x => x.VideoUrl = @"Resources\Videos\" + x.VideoUrl);
            }
            int count = _db.Posts.Where(x => x.UserId == userId).Count();
            return Ok(new { count, userPosts = posts });
        }
        [HttpGet("getAllPosts")]
        public async Task<IActionResult> GetAllPosts([FromQuery] PaginationDTO dto)
        {
            int currentSkip = dto.Skip ?? 0;
            int currentTake = dto.Take ?? 5;
            List<Post> posts = await _db.Posts
                .Include(x => x.User)
                .Include(x => x.Images)
                .Include(x => x.Videos)
                .Include(x => x.Likes)
                .Include(x => x.Comments)
                .ThenInclude(x => x.Comments)
                .ThenInclude(x => x.Likes).Skip(currentSkip).Take(currentTake).OrderByDescending(x => x.Created).ToListAsync();
            foreach (var item in posts)
            {
                item.Images.ForEach(x => x.ImageUrl = @"Resources\Images\" + x.ImageUrl);
                item.Videos.ForEach(x => x.VideoUrl = @"Resources\Videos\" + x.VideoUrl);
                if (!item.User.ImageUrl.Contains(@"Resources\Images\"))
                {
                    item.User.ImageUrl = @"Resources\Images\" + item.User.ImageUrl;
                }
            }
            int count = _db.Posts.Count();
            return Ok( new { count, allPosts = posts });
        }

    }
}
