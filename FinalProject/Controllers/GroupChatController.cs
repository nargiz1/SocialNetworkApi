using FinalProject.DAL;
using FinalProject.DTOs;
using FinalProject.Models;
using FinalProject.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupChatController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApiUser> _userManager;

        public GroupChatController(ApiDbContext db, UserManager<ApiUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] GroupChatDTO dto)
        {
            if (dto == null) return BadRequest();
            GroupChat newGroupChat = new GroupChat()
            {
                Name = dto.Name,
                Created = DateTime.Now
            };
            await _db.GroupChats.AddAsync(newGroupChat);
            await _db.SaveChangesAsync();
            foreach(string item in dto.UserIds)
            {
                ApiUser chatUser = await _userManager.FindByIdAsync(item);
                if(chatUser != null)
                {
                    GroupChatToUser groupChatToUser = new GroupChatToUser()
                    {
                        UserId = chatUser.Id,
                        GroupChatId =newGroupChat.Id
                    };
                    await _db.GroupChatToUser.AddAsync(groupChatToUser);
                    await _db.SaveChangesAsync();
                }
            }
            if(dto.ImageFile != null && Extensions.IsImage(dto.ImageFile) && Extensions.IsvalidSize(dto.ImageFile, 500))
            {
                newGroupChat.ImageUrl = await Extensions.Upload(dto.ImageFile, @"images");
                _db.GroupChats.Update(newGroupChat);
                await _db.SaveChangesAsync();
            }
            return Ok("Group created");
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] int groupId)
        {
            GroupChat chatToDelete = await _db.GroupChats.FirstOrDefaultAsync(x => x.Id == groupId);
            if (chatToDelete == null) return NotFound("Chat not found!");
            _db.GroupChats.Remove(chatToDelete);
            await _db.SaveChangesAsync();
            return Ok("Group deleted!");
        }
        //[HttpPost("update")]
        //public async Task<IActionResult> Update([FromForm] GroupChatDTO dto, int id)
        //{
        //    if (dto == null) return BadRequest();

        //}

        [HttpGet("getGroupChat")]
        public async Task<IActionResult> GetGroupChat([FromBody] int chatId)
        {
            GroupChat group = await _db.GroupChats
                .Include(x=> x.Users)
                .Include(x=> x.Messages)
                .FirstOrDefaultAsync(x => x.Id == chatId);
            if (group == null) return NotFound("Group is not found");
            return Ok(group);
        }
        //[HttpGet("getUserGroupChats")]
        //public async Task<IActionResult> GetAll([FromBody] string userId)
        //{
        //    if (userId == null) return BadRequest();
        //    ApiUser user = await _userManager.FindByIdAsync(userId);
        //    if (user == null) return NotFound("User not found!");
        //    List<GroupChatToUser> groups = await _db.GroupChatToUser.Where(x => x.UserId == userId).ToListAsync();
        //    List<GroupChat> groups = await _db.GroupChats
        //        .Include(x => x.Users)
        //        .Include(x => x.Messages).ToListAsync();
        //    return Ok(groups);
        //}
    }
}
