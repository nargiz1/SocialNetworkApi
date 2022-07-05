﻿using FinalProject.DAL;
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
                Deadline = dto.Deadline
            };
            if (dto.ImageFile != null && Extensions.IsImage(dto.ImageFile) && Extensions.IsvalidSize(dto.ImageFile, 500))
            {
                newAdv.ImageUrl = await Extensions.Upload(dto.ImageFile, "Images");
            }
            if (dto.VideoFile != null)
            {
                newAdv.VideoUrl = await Extensions.Upload(dto.VideoFile, "Videos");
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
            await Delete(advId);
            return Ok();
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] int advId)
        {
            Advertisement advToDelete = await _db.Advertisements.FirstOrDefaultAsync(x => x.Id == advId);
            if (advToDelete == null) return NotFound("Advertisement not found!");
            if(advToDelete.ImageUrl != null)
            {
                string filePath = Path.Combine(@"Resources", @"images", advToDelete.ImageUrl);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            if (advToDelete.VideoUrl != null)
            {
                string filePath = Path.Combine(@"Resources", @"videos", advToDelete.VideoUrl);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _db.Advertisements.Remove(advToDelete);
            await _db.SaveChangesAsync();
            return Ok("Advertisement deleted!");
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromForm] AdvertisementDTO dto)
        {
            if (dto == null) return BadRequest("Is null");
            Advertisement advertisementToUpdate = await _db.Advertisements.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (advertisementToUpdate == null) return NotFound("Advertisement doesn't exist");
            advertisementToUpdate.Text = dto.Text;
            if (dto.ImageFile != null && Extensions.IsImage(dto.ImageFile) && Extensions.IsvalidSize(dto.ImageFile, 500))
            {
                advertisementToUpdate.ImageUrl = await Extensions.Upload(dto.ImageFile, "Images");
            }
            if (dto.VideoFile != null && Extensions.IsVideo(dto.ImageFile) && Extensions.IsvalidSize(dto.ImageFile, 1000))
            {
                advertisementToUpdate.VideoUrl = await Extensions.Upload(dto.VideoFile, "Videos");
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
            if(!(adv.ImageUrl.Contains(@"Resources\Images\") && adv.VideoUrl.Contains(@"Resources\Videos\")))
            {
                adv.ImageUrl = (@"Resources\Images\" + adv.ImageUrl);
                adv.VideoUrl = (@"Resources\Videos\" + adv.VideoUrl);
            }
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
                if (!(item.ImageUrl.Contains(@"Resources\Images\") && item.VideoUrl.Contains(@"Resources\Videos\")))
                {
                    item.ImageUrl = (@"Resources\Images\" + item.ImageUrl);
                    item.VideoUrl = (@"Resources\Videos\" + item.VideoUrl);
                }
            }
            return Ok();
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
