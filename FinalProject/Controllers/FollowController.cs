using FinalProject.DAL;
using FinalProject.Models;
using FinalProject.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
    public class FollowController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _config;

        public FollowController(ApiDbContext db, UserManager<ApiUser> userManager, IConfiguration config)
        {
            _db = db;
            _userManager = userManager;
            _config = config;
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

            var link = "http://localhost:3000";
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new NetworkCredential("nargizramazanova28@gmail.com", _config["Mail:password"]);
            client.EnableSsl = true;
            string text = $"{user.UserName} is now following you!";
            var message = await Extensions.SendMail("socialnetworkproj1@gmail.com", userToFollow.Email, link, "Follower!", "Go to app", text);

            client.Send(message);
            message.Dispose();


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

            var link = "http://localhost:3000";
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new NetworkCredential("nargizramazanova28@gmail.com", _config["Mail:password"]);
            client.EnableSsl = true;
            string text = $"{user.UserName} unfollowed!";
            var message = await Extensions.SendMail("socialnetworkproj1@gmail.com", userToUnFollow.Email, link, "Unfollow!", "Go to app", text);

            client.Send(message);
            message.Dispose();


            return Ok("You unfollowed user!");
        }
        [HttpPost("deleteFollower")]
        public async Task<IActionResult> DeleteFollower([FromBody] string followingUserId)
        {
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);

            var followingUser = await _userManager.FindByIdAsync(followingUserId);
            if (followingUser == null) return NotFound("User not found");
            var delete = _db.FollowModels.FirstOrDefault(x => x.FollowingUserId == followingUser.Id);
            if (delete == null) return BadRequest("user is not following you");
            _db.FollowModels.Remove(delete);
            await _db.SaveChangesAsync();

            var link = "http://localhost:3000";
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new NetworkCredential("nargizramazanova28@gmail.com", _config["Mail:password"]);
            client.EnableSsl = true;
            string text = $"{user.UserName} deleted you from followers!";
            var message = await Extensions.SendMail("socialnetworkproj1@gmail.com", followingUser.Email, link, "Unfollow!", "Go to app", text);

            client.Send(message);
            message.Dispose();

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
                if (!follower.ImageUrl.Contains(@"Resources\Images\"))
                {
                    follower.ImageUrl = @"Resources\Images\" + follower.ImageUrl;
                }
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
                if (!follower.ImageUrl.Contains(@"Resources\Images\"))
                {
                    follower.ImageUrl = @"Resources\Images\" + follower.ImageUrl;
                }
                subscribes.Add(follower);
            }
            return Ok(subscribes);
        }
    }
}
