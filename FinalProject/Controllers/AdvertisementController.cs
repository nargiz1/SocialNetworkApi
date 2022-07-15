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
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class AdvertisementController : ControllerBase
    {
        private readonly ApiDbContext _db;
        public AdvertisementController(ApiDbContext db)
        {
            _db = db;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromForm] AdvertisementDTO dto)
        {
            if (dto == null) return BadRequest("Advertisement is empty");
            if (dto.Text == null && dto.ImageFile == null && dto.VideoFile == null) return BadRequest("Advertisement is empty");
            Advertisement newAdv = new Advertisement()
            {
                Text = dto.Text,
                Deadline = dto.Deadline,
                IsExpired = false
            };
            if (dto.ImageFile != null && Files.IsImage(dto.ImageFile) && Files.IsvalidSize(dto.ImageFile, 500))
            {
                newAdv.ImageUrl = Files.Upload(dto.ImageFile, "Images");
            }
            if (dto.VideoFile != null)
            {
                newAdv.VideoUrl = Files.Upload(dto.VideoFile, "Videos");
            }
            await _db.Advertisements.AddAsync(newAdv);
            await _db.SaveChangesAsync();
            return Ok(newAdv);
        }
        [HttpPost("expire")]
        public async Task<IActionResult> Expire([FromBody] int advId)
        {
            Advertisement adv = await _db.Advertisements.FirstOrDefaultAsync(x => x.Id == advId);
            if (adv == null) return NotFound("Advertisement not found!");
            await Task.Delay((int)(adv.Deadline).Subtract(DateTime.Now).TotalMilliseconds);
            adv.IsExpired = true;
            _db.Advertisements.Update(adv);
            await _db.SaveChangesAsync();
            return Ok();
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] int advId)
        {
            Advertisement advToDelete = await _db.Advertisements.FirstOrDefaultAsync(x => x.Id == advId);
            if (advToDelete == null) return NotFound("Advertisement not found!");
            if(advToDelete.ImageUrl != null)
            {
                Files.Delete(@"Resources", @"Images", advToDelete.ImageUrl);
            }
            if (advToDelete.VideoUrl != null)
            {
                Files.Delete(@"Resources", @"Videos", advToDelete.ImageUrl);
            }
            _db.Advertisements.Remove(advToDelete);
            await _db.SaveChangesAsync();
            return Ok("Advertisement deleted!");
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromForm] AdvertisementToUpdateDTO dto)
        {
            if (dto == null) return BadRequest("Is null");
            Advertisement advertisementToUpdate = await _db.Advertisements.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (advertisementToUpdate == null) return NotFound("Advertisement doesn't exist");
            advertisementToUpdate.Text = dto.Text;
            if (dto.ImageFile != null && Files.IsImage(dto.ImageFile) && Files.IsvalidSize(dto.ImageFile, 500))
            {
                advertisementToUpdate.ImageUrl = Files.Upload(dto.ImageFile, "Images");
            }
            if (dto.VideoFile != null && Files.IsVideo(dto.ImageFile) && Files.IsvalidSize(dto.ImageFile, 1000))
            {
                advertisementToUpdate.VideoUrl = Files.Upload(dto.VideoFile, "Videos");
            }
            advertisementToUpdate.Deadline = dto.Deadline;
            _db.Advertisements.Update(advertisementToUpdate);
            await _db.SaveChangesAsync();
            return Ok("Advertisement updated");
        }
        [HttpGet("getAdv")]
        [Authorize]
        public async Task<IActionResult> GetAdv([FromBody] int advId)
        {
            Advertisement adv = await _db.Advertisements.FirstOrDefaultAsync(x => x.Id == advId);
            if (adv == null) return NotFound("Advertisement not found!");
            if (!adv.ImageUrl.Contains(@"Resources\Images\")) adv.ImageUrl = @"Resources\Images\" + adv.ImageUrl;
            if (!adv.VideoUrl.Contains(@"Resources\Videos\")) adv.VideoUrl = @"Resources\Videos\" + adv.VideoUrl;
            return Ok(adv);
        }
        [HttpGet("getAll")]
        [Authorize]
        public IActionResult GetAll([FromQuery] PaginationDTO dto)
        {
            int currentSkip = dto.Skip ?? 1;
            int currentTake = dto.Take ?? 5;
            List<Advertisement> ads = _db.Advertisements.Skip(currentSkip).Take(currentTake).OrderByDescending(x => x.Created).ToList();
            foreach (var item in ads)
            {
                if(item.ImageUrl != null )
                item.ImageUrl = @"Resources\Images\" + item.ImageUrl;
                item.VideoUrl = @"Resources\Videos\" + item.VideoUrl;
            }
            return Ok( new { count = ads.Count, ads});
        }

        //[HttpPost("confirm")]
        //[Authorize(Policy = "Admin")]
        //public async Task<IActionResult> Confirm([FromBody] int advId)
        //{
        //    Advertisement advToConfirm = await _db.Advertisements.FirstOrDefaultAsync(x => x.Id == advId);
        //    if (advToConfirm == null) return NotFound("Advertisement not found!");
        //    advToConfirm.IsConfirmed = true;
        //    _db.Advertisements.Update(advToConfirm);
        //    await _db.SaveChangesAsync();
        //    return Ok("Advertisement confirmed!");
        //}
    }
}
