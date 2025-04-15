using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface IQuizService
    {
        Task<List<QuizModel>> GetQuizByCourseIdAsync(int courseId);
        Task<QuizModel> GetQuizByIdAsync(int quizId);

        Task<QuizResultModel> AddQuizResult(QuizResultRequest quizResultRequest);

        Task<List<QuizResultModel>> GetAllQuizResultByQuizIdAndUserId(int quizId, int userId);
    }
}
