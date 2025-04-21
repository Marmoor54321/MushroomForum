using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MushroomForum.Models
{
    public class Post
    {
        public int PostId { get; set; }
        [MaxLength(64)]
        public string? Title { get; set; }
        [MaxLength(512)]
        public string? Description { get; set; }
        public ForumThread? ForumThread { get; set; }
        public int? ForumThreadId { get; set; }
        public IdentityUser? User { get; set; }
        public string? IdentityUserId { get; set; }

    }
}
