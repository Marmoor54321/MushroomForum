using Microsoft.AspNetCore.Identity;

namespace MushroomForum.Models
{
    public class PostLike
    {
        public int PostLikeId { get; set; }
        public string IdentityUserId { get; set; }
        public int PostId { get; set; }
        public IdentityUser User { get; set; }
        public Post Post { get; set; }
    }
}