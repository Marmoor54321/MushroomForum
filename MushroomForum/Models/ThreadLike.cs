using Microsoft.AspNetCore.Identity;

namespace MushroomForum.Models
{
    public class ThreadLike
    {
        public int ThreadLikeId { get; set; }
        public string IdentityUserId { get; set; }
        public int ForumThreadId { get; set; }
        public IdentityUser User { get; set; }
        public ForumThread ForumThread { get; set; }
    }
}