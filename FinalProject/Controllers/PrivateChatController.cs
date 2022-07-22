using FinalProject.DAL;
using FinalProject.DTOs;
using FinalProject.Models;
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
    public class PrivateChatController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApiUser> _userManager;

        public PrivateChatController(ApiDbContext db, UserManager<ApiUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateChat([FromBody] PrivateChatDTO dto)
        {
            if (dto == null) return BadRequest();
            ApiUser userOne = await _userManager.FindByIdAsync(dto.UserOneId);
            ApiUser userTwo = await _userManager.FindByIdAsync(dto.UserTwoId);
            if (userOne == null || userTwo == null) return BadRequest();
            var duplicate = await _db.PrivateChats.FirstOrDefaultAsync(x => (x.UserOneId == userOne.Id &&
            x.UserTwoId == userTwo.Id) ||
            (x.UserOneId == userTwo.Id && x.UserTwoId == userOne.Id));
            if (duplicate != null) return BadRequest("You have chat with this user");
            PrivateChat newChat = new PrivateChat()
            {
                UserOneId = userOne.Id,
                UserTwoId = userTwo.Id,
                Created = DateTime.Now
            };
            await _db.PrivateChats.AddAsync(newChat);
            await _db.SaveChangesAsync();
            return Ok(newChat);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteChat([FromBody] int? chatId)
        {
            if (chatId == null) return NotFound();
            PrivateChat chatToDelete = await _db.PrivateChats.FirstOrDefaultAsync(x => x.Id == chatId);
            if (chatToDelete == null) return NotFound("Chat not found!");
            var messages = _db.Messages.Where(x => x.PrivateChatId == chatToDelete.Id);
            foreach (var item in messages)
            {
                _db.Messages.Remove(item);

            }
            _db.SaveChanges();
            _db.PrivateChats.Remove(chatToDelete);
            await _db.SaveChangesAsync();
            return Ok("Deleted!");
        }

        [HttpGet("getChat")]
        public async Task<IActionResult> GetChat([FromQuery] int? chatId)
        {
            if (chatId == null) return Ok();
            PrivateChat chat = await _db.PrivateChats
                .Include(x => x.UserOne)
                .Include(x => x.UserTwo)
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(x => x.Id == chatId);
            if (chat == null) return NotFound("Chat not found");
            if (!(chat.UserOne.ImageUrl.Contains(@"Resources\Images\") && chat.UserTwo.ImageUrl.Contains(@"Resources\Images\")))
            {
                chat.UserOne.ImageUrl = @"Resources\Images\" + chat.UserOne.ImageUrl;
                chat.UserTwo.ImageUrl = @"Resources\Images\" + chat.UserTwo.ImageUrl;
            }
            return Ok(chat);
        }

        [HttpGet("getUserPrivateChats")]
        public async Task<IActionResult> GetAll([FromQuery] string userId)
        {
            if (userId == null) return BadRequest();
            ApiUser user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found!");
            List<PrivateChat> chats = await _db.PrivateChats
                .Include(x => x.UserTwo)
                .Include(x => x.UserOne)
                .Where(x => x.UserOneId == user.Id || x.UserTwoId == userId)
                .ToListAsync();
            foreach (PrivateChat item in chats)
            {
                if (!(item.UserOne.ImageUrl.Contains(@"Resources\Images\") && item.UserTwo.ImageUrl.Contains(@"Resources\Images\")))
                {
                    item.UserOne.ImageUrl = @"Resources\Images\" + item.UserOne.ImageUrl;
                    item.UserTwo.ImageUrl = @"Resources\Images\" + item.UserTwo.ImageUrl;
                }
            }
            return Ok(chats);
        }
        [HttpGet("chatDoesntExists")]
        public async Task<IActionResult> ChatDoesntExists([FromQuery] string userId)
        {
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            ApiUser user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null || userId == null) return BadRequest();
            var chat = await _db.PrivateChats.FirstOrDefaultAsync(x => (x.UserOneId == user.Id &&
            x.UserTwoId == userId) ||
            (x.UserOneId == userId && x.UserTwoId == user.Id));
            if (chat != null) return BadRequest(false);
            return Ok(true);
        }
    }
}
