using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MushroomForum.Controllers;
using MushroomForum.Data;
using MushroomForum.Models;
using MushroomForum.ViewModels;
using Xunit;

public class MushroomHarvestControllerTests
{
    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var context = new ApplicationDbContext(options);

        // Dodaj przykładowe dane
        context.MushroomHarvestEntries.AddRange(new List<MushroomHarvestEntry>
        {
            new MushroomHarvestEntry { Id = 1, UserId = "user1", MushroomType = "Borowik", Quantity = 2, Date = System.DateTime.Now.AddDays(-1), Place = "Las", PhotoUrl = null },
            new MushroomHarvestEntry { Id = 2, UserId = "user1", MushroomType = "Pieczarka", Quantity = 5, Date = System.DateTime.Now, Place = "Pole", PhotoUrl = null }
        });
        context.SaveChanges();

        return context;
    }

    private MushroomHarvestController GetControllerWithUser(string userId)
    {
        var context = GetInMemoryDbContext();

        // Mock IWebHostEnvironment
        var mockEnv = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        mockEnv.Setup(m => m.WebRootPath).Returns("wwwroot");

        var controller = new MushroomHarvestController(context, mockEnv.Object);

        // Mockowanie User.Identity.NameIdentifier
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "mock"));

        controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };

        return controller;
    }

    [Fact]
    public async Task Index_ReturnsViewResult_WithCorrectModel()
    {
        // Arrange
        var controller = GetControllerWithUser("user1");

        // Act
        var result = await controller.Index(null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<MushroomHarvestIndexViewModel>(viewResult.Model);

        Assert.Equal(2, model.TotalEntries);
        Assert.Equal(1, model.PageNumber);
        Assert.Equal(8, model.PageSize);
        Assert.Equal(1, model.TotalPages);
        Assert.NotEmpty(model.Entries);
    }
}
