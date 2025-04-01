namespace MyCourse.Model
{
    public class QuestionModel
    {
        public int QuestionId { get; set; }

        public int QuizId { get; set; }

        public string QuestionText { get; set; } = null!;

        public string QuestionType { get; set; } = null!;

        public int? Points { get; set; }

        public int OrderIndex { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<AnswerModel> Answers { get; set; } = new List<AnswerModel>();
    }
}
