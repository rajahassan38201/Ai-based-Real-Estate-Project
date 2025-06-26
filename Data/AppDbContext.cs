using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PakProperties.Models;
using System.Reflection.Emit;

namespace PakProperties.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        
        public DbSet<Sell> Sell { get; set; }

        public DbSet<Rent> Rent { get; set; }
        
        public DbSet<Video> Video { get; set; }
        

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            var admin = new IdentityRole("admin")
            {
                NormalizedName = "ADMIN"
            };

            var user = new IdentityRole("user")
            {
                NormalizedName = "USER"
            };

            builder.Entity<IdentityRole>().HasData(admin,user);

            builder.Entity<Sell>()
                .HasOne(s => s.User)
                .WithMany(u => u.Properties)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Rent>()
                .HasOne(s => s.User)
                .WithMany(u => u.RentProperties)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Video>()
                .HasOne(v => v.User)
                .WithMany(u => u.Videos)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
