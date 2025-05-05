using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MushroomForum.Models
{
    public class MushroomNotes
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public string ? PhotoUrl { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
