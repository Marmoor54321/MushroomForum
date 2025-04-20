using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Models;

namespace MushroomForum.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           
            modelBuilder.Entity<ForumThread>()
                .HasOne(ft => ft.User)
                .WithMany() 
                .HasForeignKey(ft => ft.IdentityUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);


            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany() 
                .HasForeignKey(p => p.IdentityUserId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);


            modelBuilder.Entity<Post>()
                .HasOne(p => p.ForumThread)
                .WithMany()
                .HasForeignKey(p => p.ForumThreadId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
