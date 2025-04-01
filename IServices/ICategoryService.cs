using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface ICategoryService
    {
        Task<List<CategoryModel>> GetAllCategory();
    }
}
