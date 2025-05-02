using MushroomForum.Models;

namespace MushroomForum.ViewModels
{
    public class ForumThreadsIndexViewModel
    {
        public IEnumerable<ForumThread> Threads { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string SortOrder { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalThreads { get; set; }
    }
}