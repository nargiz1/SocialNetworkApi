using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> Follow([FromBody]string followedUserId)
        {
            if (followedUserId == null) return BadRequest("user cannot be null!");
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);

            var userToFollow = await _userManager.FindByIdAsync(followedUserId);
            if (userToFollow == null) return BadRequest("User not found!");

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
        public async Task<IActionResult> UnFollow([FromBody]string followedUserId)
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
        public async Task<IActionResult> DeleteFollower([FromBody]string followingUserId)
        {
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);

            var delete = _db.FollowModels.FirstOrDefault(x => x.FollowingUserId == followingUserId);
            if (delete == null) return BadRequest("user is not following you");
            _db.FollowModels.Remove(delete);
            await _db.SaveChangesAsync();
            return Ok("Follower deleted!");
        }
    }
}
