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
        public DbSet<MushroomSpot> MushroomSpots { get; set; }
        public DbSet<ThreadLike> ThreadLikes { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<UserExperience> UserExperiences { get; set; }
        public DbSet<Ciekawostka> Ciekawostki { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<UserFriend> UserFriends { get; set; }
        public DbSet<UserBlock> UserBlocks { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<AchievementType> AchievementTypes { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<LikeHistory> LikeHistories { get; set; }

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

            modelBuilder.Entity<Post>()
                .HasOne(p => p.ParentPost)
                .WithMany(p => p.Replies)
                .HasForeignKey(p => p.ParentPostId)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<UserBlock>()
                .HasOne(ub => ub.Blocker)
                .WithMany()
                .HasForeignKey(ub => ub.BlockerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserBlock>()
                .HasOne(ub => ub.Blocked)
                .WithMany()
                .HasForeignKey(ub => ub.BlockedId)
                .OnDelete(DeleteBehavior.Restrict);
   
        }
        public static void Seed(ApplicationDbContext context)
        {
            var existingQuizzes = context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .ToList();

            if (existingQuizzes.Any())
            {
                context.Quizzes.RemoveRange(existingQuizzes);
                context.SaveChanges();
            }
                var quiz = new Quiz
                {
                    Tytul = "Quiz wiedzy o grzybach",
                    Questions = new List<Question>
            {
                new Question
                {
                    Tresc = "Co jest małe i żólte?",
                    Answers = new List<Answer>
                    {
                        new Answer { Tresc = "Kurka", CzyPoprawna = true },
                        new Answer { Tresc = "Tak", CzyPoprawna = false },
                        new Answer { Tresc = "Nie", CzyPoprawna = false }
                    }
                },
                new Question
                {
                    Tresc = "Czy wszystkie grzyby można zjeść?",
                    Answers = new List<Answer>
                    {
                        new Answer { Tresc = "Tak", CzyPoprawna = true },
                        new Answer { Tresc = "Nie", CzyPoprawna = false }
                    }
                },
                new Question
                {
                    Tresc = "Czy grzyb może spleśnieć?",
                    Answers = new List<Answer>
                    {
                        new Answer { Tresc = "Tak", CzyPoprawna = true },
                        new Answer { Tresc = "Nie", CzyPoprawna = false }
                    }
                },
                new Question
                {
                    Tresc = "Kiedy jest sezon na kanie?",
                    Answers = new List<Answer>
                    {
                        new Answer { Tresc = "od czerwca do listopada", CzyPoprawna = true },
                        new Answer { Tresc = "od stycznia do maja", CzyPoprawna = false}
                    }
                },
                new Question
                {
                    Tresc = "Jakie grzyby są najfajniejsze?",
                    Answers = new List<Answer>
                    {
                        new Answer { Tresc = "Blaszkowe", CzyPoprawna = false },
                        new Answer { Tresc = "Psylocybinowe", CzyPoprawna = true },
                        new Answer { Tresc = "Rurkowe", CzyPoprawna = false }
                    }
                }
            }
                };

                context.Quizzes.Add(quiz);
                context.SaveChanges();
            }
        }

    }

