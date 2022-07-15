using FinalProject.DAL;
using FinalProject.DTOs;
using FinalProject.Models;
using FinalProject.Utils;
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
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            GroupChatToUser groupChatToUser = new GroupChatToUser()
            {
                UserId = user.Id,
                GroupChatId = newGroupChat.Id
            };
            await _db.GroupChatToUser.AddAsync(groupChatToUser);
            await _db.SaveChangesAsync();
            if(dto.ImageFile != null && Files.IsImage(dto.ImageFile) && Files.IsvalidSize(dto.ImageFile, 500))
            {
                newGroupChat.ImageUrl = Files.Upload(dto.ImageFile, "Images");
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
            if (chatToDelete.ImageUrl != null)
            {
                Files.Delete(@"Resources", @"Images", chatToDelete.ImageUrl);
            }
            var messages = _db.Messages.Where(x => x.GroupChatId == chatToDelete.Id);
            foreach (var item in messages)
            {
                _db.Messages.Remove(item);
            }
            _db.GroupChats.Remove(chatToDelete);
            await _db.SaveChangesAsync();
            return Ok("Group deleted!");
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromForm] UpdateGroupChatDTO dto)
        {
            if (dto == null) return BadRequest();
            GroupChat groupChatToUpdate = await _db.GroupChats.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (groupChatToUpdate == null) NotFound("GroupNotFound");
            groupChatToUpdate.ImageUrl = Files.Upload(dto.ImageFile, "Images");
            _db.GroupChats.Update(groupChatToUpdate);
            await _db.SaveChangesAsync();
            return Ok("Group updated!");
        }
        [HttpPost("changeName")]
        public async Task<IActionResult> ChangeName([FromBody] UpdateGroupNameDTO dto)
        {
            if (dto == null) return BadRequest();
            GroupChat groupChatToUpdate = await _db.GroupChats.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (groupChatToUpdate == null) NotFound("GroupNotFound");
            groupChatToUpdate.Name = dto.Name;
            _db.GroupChats.Update(groupChatToUpdate);
            await _db.SaveChangesAsync();
            return Ok("Group updated!");
        }

        [HttpPost("addMember")]
        public async Task<IActionResult> AddMember([FromBody] GroupMemberDTO dto)
        {
            if (dto == null) return BadRequest();
            GroupChat group = await _db.GroupChats.FirstOrDefaultAsync(x => x.Id == dto.GroupId);
            if (group == null) return NotFound("Chat was not found!");
            ApiUser userToAdd = await _userManager.FindByIdAsync(dto.UserId);
            if (userToAdd == null) return NotFound("User was not found!");
            var duplicate = await _db.GroupChatToUser.FirstOrDefaultAsync(x => x.GroupChatId == group.Id && x.UserId == userToAdd.Id);
            if (duplicate != null) return BadRequest("User is already a member of the group!");
            GroupChatToUser newGroupMember = new GroupChatToUser()
            {
                GroupChatId = group.Id,
                UserId = userToAdd.Id
            };
            await _db.GroupChatToUser.AddAsync(newGroupMember);
            await _db.SaveChangesAsync();
            return Ok("User was added to the group!");
        }

        [HttpPost("deleteMember")]
        public async Task<IActionResult> DeleteMember([FromBody] GroupMemberDTO dto)
        {
            if (dto == null) return BadRequest();
            GroupChat group = await _db.GroupChats.FirstOrDefaultAsync(x => x.Id == dto.GroupId);
            if (group == null) return NotFound("Chat was not found!");
            ApiUser userToAdd = await _userManager.FindByIdAsync(dto.UserId);
            if (userToAdd == null) return NotFound("User was not found!");
            var groupMember = await _db.GroupChatToUser.FirstOrDefaultAsync(x => x.GroupChatId == group.Id && x.UserId == userToAdd.Id);
            if (groupMember == null) return BadRequest("User is not a member of the group!");
            _db.GroupChatToUser.Remove(groupMember);
            await _db.SaveChangesAsync();
            return Ok("User was removed");
        }

        [HttpGet("getGroupChat")]
        public async Task<IActionResult> GetGroupChat([FromQuery] int? chatId)
        {
            if (chatId == null) return Ok();
            GroupChat group = await _db.GroupChats
                .Include(x=> x.Users)
                .Include(x=> x.Messages)
                .FirstOrDefaultAsync(x => x.Id == chatId);
            if (group == null) return NotFound("Group is not found");
            if (!group.ImageUrl.Contains(@"Resources\Images\"))
            {
                group.ImageUrl = @"Resources\Images\" + group.ImageUrl;
            }
            return Ok(group);
        }
        [HttpGet("getGroupMembers")]
        public async Task<IActionResult> GetGroupMembers([FromQuery] int? groupId)
        {
            if (groupId == null) return Ok();
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            GroupChat group = await _db.GroupChats
                .Include(x=> x.Users)
                .ThenInclude(X=> X.User)
                .FirstOrDefaultAsync(x => x.Id == groupId);
            if (group == null) return NotFound("Group is not found");
            List<GroupChatToUser> members = await _db.GroupChatToUser.Where(x => x.GroupChatId == group.Id && x.UserId!= user.Id).ToListAsync();
            List<ApiUser> groupMembers = new List<ApiUser>();
            foreach(var item in members)
            {
                if(item.User.ImageUrl!= null && !item.User.ImageUrl.Contains(@"Resources\Images\"))
                {
                    item.User.ImageUrl = @"Resources\Images\" + item.User.ImageUrl;
                }
                groupMembers.Add(item.User);
            }
            return Ok(groupMembers);
        }
        [HttpGet("getUserGroupChats")]
        public async Task<IActionResult> GetAll([FromQuery] string userId)
        {
            if (userId == null) return BadRequest();
            ApiUser user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found!");
            List<GroupChatToUser> groupUsers = await _db.GroupChatToUser.Where(x => x.UserId == userId).ToListAsync();
            List<GroupChat> groups = new List<GroupChat>();
            foreach(var item in groupUsers)
            {
                var group = _db.GroupChats.FirstOrDefault(x => x.Id == item.GroupChatId);
                if (!group.ImageUrl.Contains(@"Resources\Images\"))
                {
                    group.ImageUrl = @"Resources\Images\" + group.ImageUrl;
                }
                groups.Add(group);
            }
            return Ok(groups);
        }
    }
}
