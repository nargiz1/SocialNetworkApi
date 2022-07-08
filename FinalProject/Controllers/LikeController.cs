using FinalProject.DAL;
using FinalProject.Models;
using Microsoft.AspNetCore.Http;
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
    public class LikeController : ControllerBase
    {
        private readonly ApiDbContext _db;
        private readonly UserManager<ApiUser> _userManager;

        public LikeController(ApiDbContext db, UserManager<ApiUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        [HttpPost("likePost")]
        public async Task<IActionResult> LikePost([FromBody] int postId)
        {
            Post post = await _db.Posts.Include(x => x.Videos).Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null) return NotFound();
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            var duplicate = await _db.PostLikes.FirstOrDefaultAsync(x => x.PostId == post.Id && x.UserId == user.Id);
            if (duplicate != null) return BadRequest("Post already liked");
            PostLike newLike = new PostLike()
            {
                UserId = user.Id,
                PostId = post.Id,
                Created = DateTime.Now
            };

            await _db.PostLikes.AddAsync(newLike);
            await _db.SaveChangesAsync();
            return Ok("post liked");
        }
        [HttpPost("removePostLike")]
        public async Task<IActionResult> RemovePostLike([FromBody] int postId)
        {
            Post post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null) return NotFound();
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            PostLike like = await _db.PostLikes.FirstOrDefaultAsync(x => x.PostId == post.Id && x.UserId == user.Id);
            if (like == null) return NotFound();
            _db.PostLikes.Remove(like);
            await _db.SaveChangesAsync();
            return Ok("Like removed");
        }
        [HttpGet("getPostLikes")]
        public async Task<IActionResult> GetPostLikes([FromBody] int postId)
        {
            Post post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null) return NotFound();
            List<PostLike> likes = await _db.PostLikes.Include(x => x.User).Where(x => x.PostId == postId).ToListAsync();
            return Ok(likes);
        }
        [HttpPost("likeComment")]
        public async Task<IActionResult> LikeComment([FromBody] int commentId)
        {
            Comment comment = await _db.PostComments.FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null) return NotFound();
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);

            CommentLike newLike = new CommentLike()
            {
                UserId = user.Id,
                CommentId = comment.Id,
                Created = DateTime.Now
            };
            await _db.CommentLikes.AddAsync(newLike);
            await _db.SaveChangesAsync();
            return Ok("post liked");
        }
        [HttpPost("removeCommentLike")]
        public async Task<IActionResult> RemoveCommentLike([FromBody] int commentId)
        {
            Comment comment = await _db.PostComments.FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null) return NotFound();
            var userEmail = this.User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(userEmail);
            CommentLike like = await _db.CommentLikes.FirstOrDefaultAsync(x => x.Id == comment.Id && x.UserId== user.Id);
            if (like == null) return NotFound();
            _db.CommentLikes.Remove(like);
            await _db.SaveChangesAsync();
            return Ok("Like removed");
        }
        [HttpGet("getCommentLikes")]
        public async Task<IActionResult> GetCommentLikes([FromBody] int commentId)
        {
            Comment comment = await _db.PostComments.FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null) return NotFound();
            List<Comment> likes = await _db.PostComments.Include(x => x.User).Where(x => x.CommentId == commentId).ToListAsync();
            return Ok(likes);
        }
    }
}
