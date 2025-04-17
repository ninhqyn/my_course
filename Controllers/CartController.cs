using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCourse.IServices;
using MyCourse.Model;
using Microsoft.AspNetCore.Authorization;
using MyCourse.Helper;

namespace MyCourse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication at controller level
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Get all items in the user's cart
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
                // Extract user ID from token
                var userId = TokenHelper.GetUserIdFromToken(Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""));
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Token không hợp lệ" });
                }

                var items = await _cartService.GetCartItems(userId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving cart items", error = ex.Message });
            }
        }

        /// <summary>
        /// Add a course to the user's cart
        /// </summary>
        [HttpPost("add/{courseId}")]
        public async Task<IActionResult> AddToCart(int courseId)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
                var userId = TokenHelper.GetUserIdFromToken(token);

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Token không hợp lệ" });
                }

                var result = await _cartService.AddToCart(userId, courseId);

                return result switch
                {
                    AddToCartResult.Added => Ok(new { message = "Khóa học đã được thêm vào giỏ hàng." }),
                    AddToCartResult.Reactivated => Ok(new { message = "Khóa học đã được kích hoạt lại trong giỏ hàng." }),
                    AddToCartResult.AlreadyInCart => Conflict(new { message = "Khóa học đã có trong giỏ hàng." }),
                    AddToCartResult.Failed => StatusCode(500, new { message = "Đã xảy ra lỗi khi thêm vào giỏ hàng." }),
                    _ => StatusCode(500, new { message = "Không xác định được trạng thái thêm vào giỏ hàng." })
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu.", error = ex.Message });
            }
        }


        /// <summary>
        /// Remove a course from the user's cart
        /// </summary>
        [HttpDelete("remove/{courseId}")]
        public async Task<IActionResult> RemoveFromCart(int courseId)
        {
            try
            {
                // Extract user ID from token
                var userId = TokenHelper.GetUserIdFromToken(Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""));
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Token không hợp lệ" });
                }

                var result = await _cartService.RemoveFromCart(userId, courseId);
                if (result)
                {
                    return Ok(new { message = "Course removed from cart successfully" });
                }
                return NotFound(new { message = "Course not found in cart" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while removing from cart", error = ex.Message });
            }
        }

        /// <summary>
        /// Clear all items from the user's cart
        /// </summary>
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                // Extract user ID from token
                var userId = TokenHelper.GetUserIdFromToken(Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""));
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Token không hợp lệ" });
                }

                var result = await _cartService.ClearCart(userId);
                if (result)
                {
                    return Ok(new { message = "Cart cleared successfully" });
                }
                return BadRequest(new { message = "Could not clear cart" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while clearing cart", error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a course is in the user's cart
        /// </summary>
        [HttpGet("check/{courseId}")]
        public async Task<IActionResult> IsCourseInCart(int courseId)
        {
            try
            {
                // Extract user ID from token
                var userId = TokenHelper.GetUserIdFromToken(Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""));
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Token không hợp lệ" });
                }

                var result = await _cartService.IsCourseInCart(userId, courseId);
                return Ok(new { isInCart = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while checking cart", error = ex.Message });
            }
        }
    }
}