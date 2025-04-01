using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface IQuizService
    {
        Task<List<QuizModel>> GetQuizByCourseIdAsync(int courseId);
        Task<QuizModel> GetQuizByIdAsync(int quizId);
    }
}
