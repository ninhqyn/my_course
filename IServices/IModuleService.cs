using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface IModuleService
    {
        Task<List<ModuleModel>> GetAllModulesByCourseIdAsync(int courseId);

        Task<List<UserModule>> GetAllUserModuleByCourseIdAsync(int courseId,int userId);
    }
}
