using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MushroomForum.Controllers;
using MushroomForum.Models;
using MushroomForum.Services;
using Xunit;
using MushroomForum.Tests.Helpers;
namespace MushroomForum.Tests.Controllers
{
    public class PostControllerTest
    {
        [Fact]
        public async Task Index_ReturnsViewResult_WithListOfPosts()
        {
            // Arrange
            var context = DbContextHelper.GetInMemoryDbContext();
            var userManagerMock = MockHelper.GetMockUserManager();
            var achievementServiceMock = new Mock<AchievementService>(context);

            context.Posts.AddRange(
                new Post { Description = "Post 1", ForumThreadId = 1, IdentityUserId = "user-1", CreatedAt = DateTime.Now },
                new Post { Description = "Post 2", ForumThreadId = 1, IdentityUserId = "user-2", CreatedAt = DateTime.Now }
            );
            context.SaveChanges();

            var controller = new PostsController(context, userManagerMock.Object, achievementServiceMock.Object);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Post>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }
    }
}
