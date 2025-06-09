using Xunit;
using Microsoft.EntityFrameworkCore;
using Moq;
using MushroomForum.Controllers;
using MushroomForum.Models;
using MushroomForum.Data;
using MushroomForum.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

public class QuizControllerTests
{
    [Fact]
    public async Task Index_ReturnsViewWithQuizzes()
    {
        // Arrange - baza in memory
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_QuizController")
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            context.Quizzes.Add(new Quiz { Id = 1, Tytul = "Quiz 1", Questions = new List<Question>() });
            context.Quizzes.Add(new Quiz { Id = 2, Tytul = "Quiz 2", Questions = new List<Question>() });
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(options))
        {
            
            var userStoreMock = new Mock<IUserStore<IdentityUser>>();
            var userManagerMock = new Mock<UserManager<IdentityUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

           

            var controller = new QuizController(context, userManagerMock.Object, null, null);

           
            var result = await controller.Index();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Quiz>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Contains(model, q => q.Tytul == "Quiz 1");
            Assert.Contains(model, q => q.Tytul == "Quiz 2");
        }
    }
}
