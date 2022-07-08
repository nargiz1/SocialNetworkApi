using FinalProject.DAL;
using FinalProject.Models;
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
    public class FollowController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApiUser> _userManager;

        public FollowController(ApiDbContext db, UserManager<ApiUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost("follow")]
        public async Task<IActionResult> Follow([FromBody] string followedUserId)
        {
            if (followedUserId == null) return BadRequest("user cannot be null!");
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);

            var userToFollow = await _userManager.FindByIdAsync(followedUserId);
            if (userToFollow == null) return BadRequest("User not found!");
            if (userToFollow.Id == user.Id) return BadRequest("Cannot follow yourself!");
            var duplicate = await _db.FollowModels.FirstOrDefaultAsync(x => x.FollowedUserId == userToFollow.Id && x.FollowingUserId == user.Id);
            if (duplicate != null) return BadRequest("You are already following this user!");
            FollowModel newFollow = new FollowModel()
            {
                FollowingUserId = user.Id,
                FollowedUserId = followedUserId
            };
            await _db.FollowModels.AddAsync(newFollow);
            await _db.SaveChangesAsync();
            return Ok("You are following user now!");
        }
        [HttpPost("unFollow")]
        public async Task<IActionResult> UnFollow([FromBody] string followedUserId)
        {
            if (followedUserId == null) return BadRequest();
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);

            var userToUnFollow = await _userManager.FindByIdAsync(followedUserId);
            if (userToUnFollow == null) return BadRequest("User not found!");

            var delete = _db.FollowModels.FirstOrDefault(x => x.FollowedUserId == followedUserId);
            if (delete == null) return BadRequest("User not found");
            _db.FollowModels.Remove(delete);
            await _db.SaveChangesAsync();
            return Ok("You unfollowed user!");
        }
        [HttpPost("deleteFollower")]
        public async Task<IActionResult> DeleteFollower([FromBody] string followingUserId)
        {
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);

            var delete = _db.FollowModels.FirstOrDefault(x => x.FollowingUserId == followingUserId);
            if (delete == null) return BadRequest("user is not following you");
            _db.FollowModels.Remove(delete);
            await _db.SaveChangesAsync();
            return Ok("Follower deleted!");
        }
        [HttpGet("getFollowers")]
        public async Task<IActionResult> GetFollowers([FromQuery] string userId)
        {
            if (userId == null) return BadRequest("User not found");
            ApiUser user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found!");
            List<ApiUser> followers = new List<ApiUser>();
            List<FollowModel> follows = await _db.FollowModels.Where(x => x.FollowedUserId == userId).ToListAsync();
            foreach (FollowModel item in follows)
            {
                ApiUser follower = await _userManager.FindByIdAsync(item.FollowingUserId);
                followers.Add(follower);
            }
            return Ok(followers);
        }
        [HttpGet("getSubscribes")]
        public async Task<IActionResult> GetSubscribes([FromQuery] string userId)
        {
            if (userId == null) return BadRequest("User not found");
            ApiUser user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found!");
            List<ApiUser> subscribes = new List<ApiUser>();
            List<FollowModel> follows = await _db.FollowModels.Where(x => x.FollowingUserId == userId).ToListAsync();
            foreach (FollowModel item in follows)
            {
                ApiUser follower = await _userManager.FindByIdAsync(item.FollowedUserId);
                subscribes.Add(follower);
            }
            return Ok(subscribes);
        }
    }
}
