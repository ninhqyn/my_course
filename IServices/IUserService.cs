using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface IUserService
    {
        Task<UserModel> GetUser(int id);    
    }
}
