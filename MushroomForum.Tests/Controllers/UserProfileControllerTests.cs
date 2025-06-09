using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using MushroomForum.Data;
using MushroomForum.Models;

public class UserProfileControllerTests
{
    private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
    private readonly ApplicationDbContext _context;
    private readonly UserProfileController _controller;

    public UserProfileControllerTests()
    {
        _mockUserManager = MockUserManager();

        // Ustawiamy bazę in-memory EF Core - unikalna nazwa bazy dla każdego testu!
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _controller = new UserProfileController(_mockUserManager.Object, _context);

        // Mockowanie User.Identity.Name i GetUserAsync(User)
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
        new Claim(ClaimTypes.NameIdentifier, "test-user-id")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() { User = user }
        };
    }


    [Fact]
    public async Task Index_ReturnsProfile_WhenProfileExists()
    {
        // Arrange: dodajemy profil do bazy
        var identityUser = new IdentityUser { Id = "test-user-id" };

        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(identityUser);

        var existingProfile = new UserProfile
        {
            UserId = identityUser.Id,
            Level = 5,
            Experience = 50,
            AvatarIcon = "icon1.png"
        };
        _context.UserProfiles.Add(existingProfile);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserProfile>(viewResult.Model);
        Assert.Equal("test-user-id", model.UserId);
        Assert.Equal(5, model.Level);
        Assert.Equal(50, model.Experience);
        Assert.Equal("icon1.png", model.AvatarIcon);
    }

    [Fact]
    public async Task Index_CreatesNewProfile_WhenNotExists()
    {
        // Arrange: upewniamy się, że profil nie istnieje
        var userId = "test-user-id";
        _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(new IdentityUser { Id = userId });

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<UserProfile>(viewResult.Model);
        Assert.Equal(userId, model.UserId);
        Assert.Equal(1, model.Level);
        Assert.Equal(0, model.Experience);
        Assert.Equal("default.png", model.AvatarIcon);

        // Sprawdź, czy profil został dodany do bazy
        var profileInDb = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        Assert.NotNull(profileInDb);
    }

    // Mock UserManager helper
    private static Mock<UserManager<IdentityUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<IdentityUser>>();
        var mgr = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);

        return mgr;
    }
}
