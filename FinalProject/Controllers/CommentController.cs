using FinalProject.DAL;
using FinalProject.DTOs;
using FinalProject.Models;
using FinalProject.Utils;
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
    public class CommentController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _config;

        public CommentController(ApiDbContext db, UserManager<ApiUser> userManager, IConfiguration config)
        {
            _db = db;
            _userManager = userManager;
            _config = config;
        }

        [HttpPost("commentPost")]
        public async Task<IActionResult> CommentPost([FromBody] CommentDTO dto)
        {
            Post post = await _db.Posts.Include(x=> x.User).FirstOrDefaultAsync(x => x.Id == dto.PostId);
            if (post == null) return NotFound();
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (string.IsNullOrEmpty(dto.Text)) return BadRequest("Comment cannot be empty");
            Comment newComment = new Comment()
            {
                UserId = user.Id,
                Text = dto.Text,
                PostId = post.Id,
                Created = DateTime.Now,
            };
            await _db.PostComments.AddAsync(newComment);
            await _db.SaveChangesAsync();

            var link = "http://localhost:3000";
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new NetworkCredential("nargizramazanova28@gmail.com", _config["Mail:password"]);
            client.EnableSsl = true;
            string text = $"{user.UserName} commented your post!";
            var message = await Extensions.SendMail("socialnetworkproj1@gmail.com", post.User.Email, link, "Comment!", "Go to app", text);

            client.Send(message);
            message.Dispose();

            return Ok("Comment saved!");
        }
        [HttpPost("commentComment")]
        public async Task<IActionResult> CommentComment([FromBody] CommentDTO dto)
        {
            Comment comment = await _db.PostComments.FirstOrDefaultAsync(x => x.Id == dto.CommentId);
            if (comment == null) return NotFound();
            Post post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == comment.PostId);
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);

            if (string.IsNullOrEmpty(dto.Text)) return BadRequest("Comment cannot be empty");
            Comment newComment = new Comment()
            {
                UserId = user.Id,
                Text = dto.Text,
                PostId = post.Id,
                CommentId = comment.Id,
                Created = DateTime.Now,
            };
            await _db.PostComments.AddAsync(newComment);
            await _db.SaveChangesAsync();
            return Ok("Comment saved!");
        }

        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] int commentId)
        {
            var commentToDelete = await _db.PostComments.Include(x=> x.Comments).FirstOrDefaultAsync(x => x.Id == commentId);
            if (commentToDelete == null) return NotFound("Comment not found!");
            _db.PostComments.Remove(commentToDelete);
            await _db.SaveChangesAsync();
            return Ok("Comment deleted");
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateCommentDTO updatedComment)
        {
            var commentToUpdate = await _db.PostComments.FirstOrDefaultAsync(x => x.Id == updatedComment.Id);
            if (commentToUpdate == null) return NotFound("Comment not found!");
            commentToUpdate.Text = updatedComment.Text;
            commentToUpdate.Updated = DateTime.Now;
            _db.PostComments.Update(commentToUpdate);
            await _db.SaveChangesAsync();
            return Ok("Comment updated!");
        }

        [HttpGet("getPostComments")]
        public async Task<IActionResult> GetPostComments([FromBody] int postId)
        {
            Post post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null) return NotFound();
            List<Comment> comments = await _db.PostComments
                .Include(x=> x.User)
                .Include(x => x.Comments)
                .Where(x=> x.PostId == postId)
                .OrderByDescending(x=> x.Created).ToListAsync();
            foreach(var item in comments)
            {
                if (!item.User.ImageUrl.Contains(@"Resources\Images\"))
                {
                    item.User.ImageUrl = @"Resources\Images\" + item.User.ImageUrl;
                }
            }
            return Ok(comments);
        }
        [HttpGet("getComment")]
        public async Task<IActionResult> GetComment([FromBody] int commentId)
        {
            Comment comment = await _db.PostComments
                .Include(x=> x.User)
                .FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null) return NotFound();
            if (!comment.User.ImageUrl.Contains(@"Resources\Images\"))
            {
                comment.User.ImageUrl = @"Resources\Images\" + comment.User.ImageUrl;
            }
            return Ok(comment);
        }
    }
}
