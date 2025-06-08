using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MushroomForum.Controllers;
using MushroomForum.Data;
using MushroomForum.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MushroomForum.Tests.Controllers
{
    public class MushroomSpotsControllerTests
    {
        [Fact]
        public async Task Index_ReturnsViewResult_WithUserSpots()
        {
            // Arrange - db context in memory
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase_MushroomSpots")
                .Options;

            using (var context = new ApplicationDbContext(options))
            {
                context.MushroomSpots.AddRange(
                    new MushroomSpot { 
                        Name = "Spot 1",
                        Description = "Opis 1",
                        UserId = "user-123", 
                        Latitude = 50.1, 
                        Longitude = 19.9, 
                        Rating = 4 },
                    
                    
                    new MushroomSpot { Name = "Spot 2", 
                        UserId = "user-999",
                        Description = "Opis 2",
                        Latitude = 51.2, 
                        Longitude = 20.0, 
                        Rating = 5 }
                );
                context.SaveChanges();
            }

            using (var context = new ApplicationDbContext(options))
            {
                // Mock UserManager
                var userStoreMock = new Mock<IUserStore<IdentityUser>>();
                var userManagerMock = new Mock<UserManager<IdentityUser>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
                userManagerMock.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns("user-123");

                // Controller
                var controller = new MushroomSpotsController(context, userManagerMock.Object);

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
                var model = Assert.IsAssignableFrom<IEnumerable<MushroomSpot>>(viewResult.Model);

                // Should return only one spot for user-123
                Assert.Single(model);
                Assert.Equal("Spot 1", model.First().Name);
            }
        }
    }
}
