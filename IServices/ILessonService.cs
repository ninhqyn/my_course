using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface ILessonService
    {
        Task<List<LessonModel>> GetLessonByModuleIdAsync(int moduleId);
        Task<LessonModel> GetLessonModuleById(int lessonId);
        Task<LessonProgressModel> GetLessonProgressByLessonIdAndUserId(int lessonId, int userId);

        Task<LessonProgressModel> CreateOrUpdateLessonProgressAsync(int userId,CreateProgressModel createProgress);

        Task<List<LessonProgressModel>> GetLessonProgressByUserId(int userId,int courseId);
    }
}
