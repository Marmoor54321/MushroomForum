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
        //DbSet rejestruje i tworzy tabelę w bazie danych o nazwie ForumThreads, Posts itd.
        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<MushroomNotes> MushroomNotes { get; set; }
        public DbSet<ThreadLike> ThreadLikes { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<UserFriend> UserFriends { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ForumThread>()
                .HasOne(ft => ft.User)
                .WithMany()
                .HasForeignKey(ft => ft.IdentityUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.IdentityUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.ForumThread)
                .WithMany()
                .HasForeignKey(p => p.ForumThreadId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ThreadLike>()
                .HasIndex(tl => new { tl.IdentityUserId, tl.ForumThreadId })
                .IsUnique();

            modelBuilder.Entity<PostLike>()
                .HasIndex(pl => new { pl.IdentityUserId, pl.PostId })
                .IsUnique();

            modelBuilder.Entity<ThreadLike>()
                .HasOne(tl => tl.ForumThread)
                .WithMany()
                .HasForeignKey(tl => tl.ForumThreadId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PostLike>()
               .HasOne(pl => pl.Post)
               .WithMany()
               .HasForeignKey(pl => pl.PostId)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Sender)
                .WithMany()
                .HasForeignKey(fr => fr.SenderId)
                .OnDelete(DeleteBehavior.NoAction);
            

            modelBuilder.Entity<FriendRequest>()
                .HasOne(fr => fr.Receiver)
                .WithMany()
                .HasForeignKey(fr => fr.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade);

        }

    }
}
