using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MushroomForum.Models
{
    public class ForumThread
    {
        //Primary key
        public int ForumThreadId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        //Navigation property
        [ValidateNever]
        public IdentityUser User { get; set; }
        //FK
        public string IdentityUserId { get; set; }
        public Category? Category { get; set; }
        
        public int? CategoryId { get; set; }
    }
}
