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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StoryController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApiUser> _userManager;

        public StoryController(ApiDbContext db, UserManager<ApiUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create ([FromForm] StoryDTO dto)
        {
            if (dto.File == null) return BadRequest("Story cannot be empty");
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            ApiUser user = await _userManager.FindByEmailAsync(userEmail);
            Story newStory = new Story()
            {
                UserId = user.Id,
                Created = DateTime.Now
            };
            if (dto.File.IsImage() && Files.IsvalidSize(dto.File, 500)) newStory.ImageUrl = Files.Upload(dto.File, "Images");
            if (dto.File.IsVideo() && Files.IsvalidSize(dto.File, 1000)) newStory.VideoUrl = Files.Upload(dto.File, "Videos");
            await _db.Stories.AddAsync(newStory);
            await _db.SaveChangesAsync();
            return Ok(newStory);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] int? storyId)
        {
            if (storyId == null) return BadRequest();
            Story storyToDelete = await _db.Stories.FirstOrDefaultAsync(x => x.Id == storyId);
            if (storyToDelete == null) return NotFound("Story not founfd");
            if(storyToDelete.ImageUrl != null) Files.Delete(@"Resources", @"Images", storyToDelete.ImageUrl);
            if(storyToDelete.VideoUrl != null) Files.Delete(@"Resources", @"Videos", storyToDelete.VideoUrl);
            _db.Stories.Remove(storyToDelete);
            await _db.SaveChangesAsync();
            return Ok("story deleted");
        }
        [HttpPost("story")]
        public async Task<IActionResult> ExpireStory([FromBody] int storyId)
        {
            Story story = await _db.Stories.FirstOrDefaultAsync(x => x.Id == storyId);
            if (story == null) return NotFound("Story not found!");
            await Task.Delay((int)(DateTime.Now.AddSeconds(120)).Subtract(DateTime.Now).TotalMilliseconds);
            if (story.ImageUrl != null) Files.Delete(@"Resources", @"Images", story.ImageUrl);
            if (story.VideoUrl != null) Files.Delete(@"Resources", @"Videos", story.VideoUrl);
            await Delete(story.Id);
            return Ok();
        }
        [HttpGet("getStory")]
        public async Task<IActionResult> GetSory([FromQuery] int? storyId)
        {
            if (storyId == null) return BadRequest();
            Story story = await _db.Stories.Include(x=> x.User).FirstOrDefaultAsync(x => x.Id == storyId);
            if (story.ImageUrl != null && !story.ImageUrl.Contains(@"Resources\Images")) story.ImageUrl = @"Resources\Images\" + story.ImageUrl;
            if (story.VideoUrl != null && !story.VideoUrl.Contains(@"Resources\Videos")) story.VideoUrl = @"Resources\Videos\" + story.VideoUrl;
            if (story == null) return NotFound("Story not found");
            return Ok(story);
        }
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            List<Story> stories = await _db.Stories.ToListAsync();
            stories.ForEach(x => x.ImageUrl = @"Resources\Images\" + x.ImageUrl);
            stories.ForEach(x => x.VideoUrl = @"Resources\Videos\" + x.VideoUrl);
            return Ok(stories);
        }

    }
}
