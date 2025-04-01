namespace MyCourse.Model
{
    public class AnswerModel
    {
        public int AnswerId { get; set; }

        public int QuestionId { get; set; }

        public string AnswerText { get; set; } = null!;

        public bool? IsCorrect { get; set; }

        public int? Points { get; set; }

        public string? Explanation { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
