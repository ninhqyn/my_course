using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface ISkillService
    {
        Task<List<SkillModel>> GetSkillsByCourseIdAsync(int courseId);
    }
}
