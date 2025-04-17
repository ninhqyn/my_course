using AutoMapper;
using MyCourse.Data;
using MyCourse.IServices;
using MyCourse.Model;
using Microsoft.EntityFrameworkCore;
using MyCourse.Helper;

namespace MyCourse.Services
{
    public class CartService : ICartService
    {
        private readonly MyCourseContext _context;
        private readonly IMapper _mapper;
        public CartService(MyCourseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AddToCartResult> AddToCart(int userId, int courseId)
        {
            try
            {
                var existingItem = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);

                if (existingItem != null)
                {
                    if (!existingItem.IsActive ?? false)
                    {
                        existingItem.IsActive = true;
                        existingItem.AddedAt = DateTime.Now;
                        await _context.SaveChangesAsync();
                        return AddToCartResult.Reactivated;
                    }

                    return AddToCartResult.AlreadyInCart;
                }

                var cartItem = new Cart
                {
                    UserId = userId,
                    CourseId = courseId,
                    AddedAt = DateTime.Now,
                    IsActive = true
                };

                await _context.Carts.AddAsync(cartItem);
                await _context.SaveChangesAsync();
                return AddToCartResult.Added;
            }
            catch (Exception)
            {
                return AddToCartResult.Failed;
            }
        }


        public async Task<bool> ClearCart(int userId)
        {
            try
            {
                // Find all active cart items for user
                var cartItems = await _context.Carts
                    .Where(c => c.UserId == userId && c.IsActive == true)
                    .ToListAsync();

                // Mark all items as inactive
                foreach (var item in cartItems)
                {
                    item.IsActive = false;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<CartModel>> GetCartItems(int userId)
        {
            var cartItems = await _context.Carts
                .Include(c => c.Course)
                .ThenInclude(c=>c.Category)
                .Where(c => c.UserId == userId && c.IsActive == true)
                .ToListAsync();

            return _mapper.Map<List<CartModel>>(cartItems);
        }

        public async Task<bool> IsCourseInCart(int userId, int courseId)
        {
            return await _context.Carts
                .AnyAsync(c => c.UserId == userId && c.CourseId == courseId && c.IsActive == true);
        }

        public async Task<bool> RemoveFromCart(int userId, int courseId)
        {
            try
            {
                var cartItem = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId && c.IsActive == true);

                if (cartItem == null)
                    return false;

                // Mark as inactive instead of deleting
                cartItem.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}