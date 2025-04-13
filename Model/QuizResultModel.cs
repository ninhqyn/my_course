namespace MyCourse.Model
{
    public class QuizResultModel
    {
        public int ResultId { get; set; }
        public decimal Score { get; set; }
        public bool Passed { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime SubmissionDate { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
    }
}