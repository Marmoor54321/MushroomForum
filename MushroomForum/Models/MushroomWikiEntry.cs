using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MushroomForum.Models
{
    public class MushroomWikiEntry
    {
        public int Id { get; set; }
        [MaxLength(32)]
        public string Name { get; set; }
        [MaxLength(64)]
        public string LatinName { get; set; }
        [MaxLength(256)]
        public string Description { get; set; }
        [MaxLength(32)]
        public string Type { get; set; } 
        public DateTime Date { get; set; } = DateTime.Now;
        public string? PhotoUrl { get; set; }

        public string? UserId { get; set; }

        public IdentityUser? User { get; set; }
        public string? WikiUrl { get; set; }
    }
}