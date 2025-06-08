using Microsoft.AspNetCore.Identity;
using Moq;

namespace MushroomForum.Tests.Helpers
{
    public static class MockHelper
    {
        public static Mock<UserManager<IdentityUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }
    }
}
