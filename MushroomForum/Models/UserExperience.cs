using Microsoft.AspNetCore.Identity;

namespace MushroomForum.Models
{
    public class UserExperience
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
        public int Doswiadczenie { get; set; } = 0;
    }
}
