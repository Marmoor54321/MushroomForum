using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MushroomForum.Controllers;
using MushroomForum.Data;
using MushroomForum.Models;
using MushroomForum.ViewModels;
using Xunit;

namespace MushroomForum.Tests
{
    public class MushroomWikiControllerTest
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        private MushroomWikiController CreateControllerWithUser(ApplicationDbContext context, string userId = "user1")
        {
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath).Returns(AppContext.BaseDirectory);

            var controller = new MushroomWikiController(context, envMock.Object);

            // Mockowanie User.Identity z Claim
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithPaginatedEntries()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            // Dodaj 20 wpisów testowych
            for (int i = 1; i <= 20; i++)
            {
                context.MushroomWikiEntries.Add(new MushroomWikiEntry
                {
                    Id = i,
                    Name = $"Mushroom {i}",
                    Description = $"Description {i}",
                    LatinName = $"LatinName {i}",
                    Type = "Edible",
                    Date = DateTime.Now.AddDays(-i)
                });
            }

            await context.SaveChangesAsync();

            var controller = CreateControllerWithUser(context);

            // Act
            var result = await controller.Index(pageNumber: 2, pageSize: 8);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<MushroomWikiIndexViewModel>(viewResult.Model);

            Assert.Equal(2, model.PageNumber);
            Assert.Equal(8, model.PageSize);
            Assert.Equal(20, model.TotalEntries);
            Assert.Equal(3, model.TotalPages); // 20 / 8 -> 3 strony
            Assert.Equal(8, model.Entries.Count);
            Assert.Equal("Mushroom 9", model.Entries.First().Name); // kolejność malejąca po dacie
        }

        [Fact]
        public void Create_Get_ReturnsViewResult_WithEmptyEntry()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = CreateControllerWithUser(context);

            // Act
            var result = controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<MushroomWikiEntry>(viewResult.Model);
        }

        // Można też napisać test POST Create z mockiem IFormFile itd.
        // Dla uproszczenia przykład testu Create POST z poprawnym modelem i bez zdjęcia:

        [Fact]
        public async Task Create_Post_ValidModel_SavesEntryAndRedirects()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = CreateControllerWithUser(context, "testuser");

            var newEntry = new MushroomWikiEntry
            {
                Name = "Test Mushroom",
                LatinName = "Testus fungius",
                Description = "Description",
                Date = default, // ma ustawić na DateTime.Now
                Type = "Edible"
            };

            // Act
            var result = await controller.Create(newEntry, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(controller.Index), redirectResult.ActionName);

            var savedEntry = context.MushroomWikiEntries.FirstOrDefault(e => e.Name == "Test Mushroom");
            Assert.NotNull(savedEntry);
            Assert.Equal("testuser", savedEntry.UserId);
            Assert.NotEqual(default, savedEntry.Date);
            Assert.Equal("/images/default-wiki-mushroom.jpg", savedEntry.PhotoUrl);
        }
    }
}
