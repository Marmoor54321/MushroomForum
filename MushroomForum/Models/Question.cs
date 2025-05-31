namespace MushroomForum.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Tresc { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public ICollection<Answer> Answers { get; set; }
    }
}
