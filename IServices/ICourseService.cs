using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface ICourseService
    {
        Task<List<CourseModel>> GetAllCourse();
        Task<CourseModel> GetCourse(int id);
        Task<CourseModel> CreateCourse(CourseModel courseModel);
        Task<CourseModel> UpdateCourse(int id, CourseModel courseModel);
        Task<bool> DeleteCourse(int id);
        Task<List<CourseModel>> GetAllCourseByCategoryId(int categoryId,int page,int pageSize);
        
        Task<List<CourseModel>> GetAllFavoriteCourse(int page,int pageSize);
        Task<List<CourseModel>> GetAllNewCourse(int page,int pageSize);
        Task<List<CourseModel>> GetAllCourseByFilter(string keyword, int categoryId, int instructorId, int page, int pageSize);

        Task<List<UserCourse>> GetAllCourseByUserId(int userId);
     
    }
}