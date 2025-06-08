using Xunit;
using MushroomForum.Controllers;
using MushroomForum.Data;
using MushroomForum.Models;
using MushroomForum.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MushroomForum.Tests.Controllers
{
    public class ForumThreadsControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewResult_WithForumThreadsIndexViewModel()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_ForumThreads")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                var category = new Category { Name = "Test Category" };
                context.Categories.Add(category);

                context.ForumThreads.Add(new ForumThread
                {
                    Title = "Test Thread",
                    Description = "Test Description", // <--- Tutaj jest konieczne!
                    CreatedAt = DateTime.Now,
                    Category = category
                });

                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                var controller = new ForumThreadsController(context);

                var result = await controller.Index(1, null, null, null, null);

                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<ForumThreadsIndexViewModel>(viewResult.Model);
                Assert.Single(model.Threads);
                Assert.Single(model.Categories);
            }
        }
    }
}
