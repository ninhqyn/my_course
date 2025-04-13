namespace MyCourse.Model
{
    public class QuizResultRequest
    {
        public int UserId { get; set; }
        public int QuizId { get; set; }
        public List<QuizAnswerRequest> Answers { get; set; }
    }

    public class QuizAnswerRequest
    {
        public int QuestionId { get; set; }
        public int AnswerId { get; set; }
    }

}
