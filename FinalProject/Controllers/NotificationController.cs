using FinalProject.DAL;
using FinalProject.DTOs;
using FinalProject.Models;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApiUser> _userManager;

        public NotificationController(ApiDbContext db, UserManager<ApiUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create ([FromBody] NotificationsDTO dto)
        {
            if (dto == null) return BadRequest();
            Notification newNotification = new Notification()
            {
                FromUserId = dto.FromUserId,
                ToUserId = dto.ToUserId,
                Text = dto.Text,
                Created = DateTime.Now,
                IsRead = dto.IsRead
            };
            await _db.Notifications.AddAsync(newNotification);
            await _db.SaveChangesAsync();
            return Ok(newNotification);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] int? notificationId)
        {
            if (notificationId == null) return BadRequest();
            Notification notification = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId);
            if (notification == null) return NotFound("Notification is not found!");
            _db.Notifications.Remove(notification);
            await _db.SaveChangesAsync();
            return Ok("Notification deleted");
        }
        [HttpGet("getUserNotifications")]
        public async Task<IActionResult> GetAll([FromQuery] string userId)
        {
            if (userId == null) return BadRequest();
            ApiUser user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User is not found");
            List<Notification> notifications = await _db.Notifications.Where(x=> x.ToUserId == user.Id).ToListAsync();
            return Ok(notifications);
        }
    }
}
