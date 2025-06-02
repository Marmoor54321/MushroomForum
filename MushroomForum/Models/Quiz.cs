namespace MushroomForum.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Tytul { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}
