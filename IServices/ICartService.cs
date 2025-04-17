using MyCourse.Helper;
using MyCourse.Model;

namespace MyCourse.IServices
{
    public interface ICartService
    {
        Task<AddToCartResult> AddToCart(int userId, int courseId);
        Task<bool> RemoveFromCart(int userId, int courseId);
        Task<List<CartModel>> GetCartItems(int userId);
        Task<bool> ClearCart(int userId);
        Task<bool> IsCourseInCart(int userId, int courseId);
   

    }
}
