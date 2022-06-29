﻿using FinalProject.DAL;
using FinalProject.DTOs;
using FinalProject.Models;
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
            PrivateChat newChat = new PrivateChat()
            {
                UserOneId = userOne.Id,
                UserTwoId = userTwo.Id,
                Created = DateTime.Now
            };
            await _db.PrivateChats.AddAsync(newChat);
            await _db.SaveChangesAsync();
            return Ok("Chat created!");
        }
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteChat([FromBody] int chatId)
        {
            PrivateChat chatToDelete = await _db.PrivateChats.FirstOrDefaultAsync(x => x.Id == chatId);
            if (chatToDelete == null) return NotFound("Chat not found!");
            _db.PrivateChats.Remove(chatToDelete);
            await _db.SaveChangesAsync();
            return Ok("Chat deleted!");
        }
        [HttpGet("getChat")]
        public async Task<IActionResult> GetChat([FromBody] int chatId)
        {
            PrivateChat chat = await _db.PrivateChats
                .Include(x=> x.UserOne)
                .Include(x=> x.UserTwo)
                .Include(x=> x.Messages)
                .FirstOrDefaultAsync(x => x.Id == chatId);
            if (chat == null) return NotFound("Chat not found");
            return Ok(chat);
        }
        [HttpGet("getUserPrivateChats")]
        public async Task<IActionResult> GetAll([FromBody] string userId)
        {
            if (userId == null) return BadRequest();
            ApiUser user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found!");
            List<PrivateChat> chats = await _db.PrivateChats
                .Where(x => x.UserOneId == user.Id || x.UserTwoId == userId)
                .Include(x=> x.UserTwo)
                .Include(x=> x.UserOne)
                .ToListAsync();
            return Ok(chats);
        }
    }
}