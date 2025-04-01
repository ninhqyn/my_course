namespace MyCourse.Model
{
    public class QuizModel
    {
        public int QuizId { get; set; }

        public int ModuleId { get; set; }

        public string QuizName { get; set; } = null!;

        public string? Description { get; set; }

        public int? PassingScore { get; set; }

        public int? TimeLimitMinutes { get; set; }

        public int OrderIndex { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
    }
}
