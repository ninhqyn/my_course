using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface IRatingService
    {
        Task<RatingTotalMoel> GetRatingTotalByCourseId(int courseId);
        Task<List<RatingModel>> GetRatingByCourseId(int courseId, int page = 1, int pageSize = 10);
    }
}
