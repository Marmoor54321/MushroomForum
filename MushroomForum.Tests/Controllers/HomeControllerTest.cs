using Xunit;
using Moq;
using MushroomForum.Controllers;
using MushroomForum.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MushroomForum.Tests.Controllers
{
    public class HomeControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResult_WithExpectedViewData()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<HomeController>>();
            var ciekawostkiService = new CiekawostkiService(); // prawdziwa klasa, bo nie mamy interfejsu

            var controller = new HomeController(loggerMock.Object, ciekawostkiService);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.NotNull(controller.ViewData["Ciekawostka"]);
            Assert.NotNull(controller.ViewData["CiekawostkaImg"]);
            Assert.NotNull(controller.ViewData["Mem"]);

            // dodatkowo możesz sprawdzić typ:
            Assert.IsType<string>(controller.ViewData["Ciekawostka"]);
            Assert.IsType<string>(controller.ViewData["CiekawostkaImg"]);
            Assert.IsType<string>(controller.ViewData["Mem"]);
        }
    }
}
