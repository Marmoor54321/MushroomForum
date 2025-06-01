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
        }
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Quizzes.Any())
            {
                var quiz = new Quiz
                {
                    Tytul = "Testowy quiz o grzybach",
                    Questions = new List<Question>
            {
                new Question
                {
                    Tresc = "Jakiego koloru jest muchomor czerwony?",
                    Answers = new List<Answer>
                    {
                        new Answer { Tresc = "Czerwony", CzyPoprawna = true },
                        new Answer { Tresc = "Zielony", CzyPoprawna = false },
                        new Answer { Tresc = "Niebieski", CzyPoprawna = false }
                    }
                },
                new Question
                {
                    Tresc = "Czy boczniak jest jadalny?",
                    Answers = new List<Answer>
                    {
                        new Answer { Tresc = "Tak", CzyPoprawna = true },
                        new Answer { Tresc = "Nie", CzyPoprawna = false }
                    }
                },
                new Question
                {
                    Tresc = "Jak nazywa się trujący grzyb znany jako muchomor zielonawy?",
                    Answers = new List<Answer>
                    {
                        new Answer { Tresc = "Muchomor sromotnikowy", CzyPoprawna = true },
                        new Answer { Tresc = "Borowik", CzyPoprawna = false }
                    }
                },
                new Question
                {
                    Tresc = "Czy kania jest grzybem jadalnym?",
                    Answers = new List<Answer>
                    {
                        new Answer { Tresc = "Tak", CzyPoprawna = true },
                        new Answer { Tresc = "Nie", CzyPoprawna = false }
                    }
                },
                new Question
                {
                    Tresc = "Który z grzybów jest trujący?",
                    Answers = new List<Answer>
                    {
                        new Answer { Tresc = "Pieczarka", CzyPoprawna = false },
                        new Answer { Tresc = "Gąska zielonka", CzyPoprawna = true }
                    }
                }
            }
                };

                context.Quizzes.Add(quiz);
                context.SaveChanges();
            }
        }

    }
}
