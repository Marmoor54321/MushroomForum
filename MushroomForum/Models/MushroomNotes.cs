namespace MushroomForum.Models
{
    public class MushroomNotes
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ? PhotoUrl { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
