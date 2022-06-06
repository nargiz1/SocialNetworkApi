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
    }
}
