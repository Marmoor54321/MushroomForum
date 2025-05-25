using MushroomForum.Models;

namespace MushroomForum.ViewModels
{
    public class ThreadDetailsViewModel
    {
        public ForumThread Thread { get; set; }
        public List<Post> Posts { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public int TotalPosts { get; set; }
        public int PageSize { get; set; } = 10;
        public int ThreadLikeCount { get; set; }
        public Dictionary<int, int> PostLikeCounts { get; set; } = new();
        public HashSet<int> LikedPostIds { get; set; } = new();
        public bool ThreadLikedByCurrentUser { get; set; }
    }
}