namespace MushroomForum.Models
{

    public class Answer
    {
        public int Id { get; set; }
        public string Tresc { get; set; }
        public bool CzyPoprawna { get; set; }
        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
