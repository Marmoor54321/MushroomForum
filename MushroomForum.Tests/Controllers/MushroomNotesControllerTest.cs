using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Controllers;
using MushroomForum.Data;
using MushroomForum.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MushroomForum.Tests.Controllers
{
    public class MushroomNotesControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewResult_WithUserNotes()
        {
            // Arrange - db context in memory
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_MushroomNotes")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.MushroomNotes.AddRange(
                    new MushroomNotes { Title = "Note 1", Content = "Content 1", UserId = "user-123", CreateDate = DateTime.Now },
                    new MushroomNotes { Title = "Note 2", Content = "Content 2", UserId = "user-999", CreateDate = DateTime.Now }
                );
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                // Mock UserManager
                var userStoreMock = new Mock<IUserStore<IdentityUser>>();
                var userManagerMock = new Mock<UserManager<IdentityUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
                userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

                // Mock IWebHostEnvironment
                var envMock = new Mock<IWebHostEnvironment>();

                // Controller
                var controller = new MushroomNotesController(context, envMock.Object, userManagerMock.Object);

                // Fake user (so controller.User works)
                var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "user-123")
                }, "mock"));
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                };

                // Act
                var result = await controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<MushroomNotes>>(viewResult.Model);

                // Should return only one note for user-123
                Assert.Single(model);
                Assert.Equal("Note 1", model.First().Title);
            }
        }
    }
}