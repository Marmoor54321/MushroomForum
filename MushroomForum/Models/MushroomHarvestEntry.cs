using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MushroomForum.Models
{
    public class MushroomHarvestEntry
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Typ grzyba jest wymagany.")]
        public string MushroomType { get; set; }

        [Required(ErrorMessage = "Ilość jest wymagana.")]
        [Range(1, int.MaxValue, ErrorMessage = "Ilość musi być większa od 0.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Data jest wymagana.")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Miejsce jest wymagane.")]
        public string Place { get; set; }

        public string? PhotoUrl { get; set; }

        public string? UserId { get; set; }

        [ValidateNever]
        public IdentityUser? User { get; set; }
    }
}