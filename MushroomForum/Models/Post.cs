using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace MushroomForum.Models
{
    public class Post
    {
        public int PostId { get; set; }
        [MaxLength(512)]
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ForumThread? ForumThread { get; set; }
        public int? ForumThreadId { get; set; }
        public IdentityUser? User { get; set; }
        public string? IdentityUserId { get; set; }
        [AllowNull]
        public ICollection<Media> Media { get; set; } = new List<Media>();

        //Replies
        public int? ParentPostId { get; set; }
        public Post? ParentPost { get; set; }
        public ICollection<Post> Replies { get; set; } = new List<Post>();
    }
}
