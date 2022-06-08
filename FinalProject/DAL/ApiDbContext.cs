using FinalProject.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinalProject.DAL
{
    public class ApiDbContext : IdentityDbContext<ApiUser>
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {}
        public DbSet<SocialMediaLink> SocialMediaLinks { get; set; }
        public DbSet<FollowModel> FollowModels { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<PostImage> PostImages { get; set; }
        public DbSet<PostVideo> PostVideos { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<CommentComment> CommentComments { get; set; }
    }
}
