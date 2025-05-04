using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MushroomForum.Models
{
    public class ForumThread
    {
        //Primary key
        public int ForumThreadId { get; set; }
        [MaxLength(64)]
        public string Title { get; set; }
        [MaxLength(512)]
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        //Navigation property
        [ValidateNever]
        public IdentityUser? User { get; set; }
        //FK
        public string? IdentityUserId { get; set; }
        public Category? Category { get; set; }
        
        public int? CategoryId { get; set; }
    }
}
