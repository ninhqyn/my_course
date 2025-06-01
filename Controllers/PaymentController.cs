using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCourse.IServices;
using MyCourse.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCourse.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize] // Yêu cầu người dùng phải được xác thực (có token hợp lệ)
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<PaymentModel>>> GetPaymentHistory(int page = 1, int pageSize = 10)
        {
            // Lấy token từ header Authorization
            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return Unauthorized(); // Trả về 401 Unauthorized nếu không có token
            }
            Console.WriteLine("pageSize:" + pageSize);
            // Lấy UserId từ token sử dụng TokenHelper
            int userId = TokenHelper.GetUserIdFromToken(authorizationHeader);
            if (userId == 0)
            {
                return Unauthorized(); // Trả về 401 Unauthorized nếu không lấy được UserId hợp lệ từ token
            }
            var paymentHistory = await _paymentService.GetPaymentHistoryByUserId(userId, page, pageSize);
            return Ok(paymentHistory);
        }
        /// <summary>
        /// Lấy thông tin chi tiết của một thanh toán theo ID.
        /// </summary>
        /// <param name="paymentId">ID của thanh toán.</param>
        /// <returns>Một ActionResult chứa PaymentModel hoặc NotFound nếu không tìm thấy.</returns>
        [HttpGet("{paymentId}")]
        public async Task<ActionResult<PaymentModel>> GetPaymentById(int paymentId)
        {
            // Ở đây, bạn có thể muốn thêm logic kiểm tra xem người dùng hiện tại có quyền xem thanh toán này hay không.
            // Ví dụ: kiểm tra xem Payment.UserId có khớp với UserId từ token hay không.

            var payment = await _paymentService.GetPaymentById(paymentId);
            if (payment == null)
            {
                return NotFound();
            }
            return Ok(payment);
        }

        /// <summary>
        /// Xóa một thanh toán theo ID.
        /// </summary>
        /// <param name="paymentId">ID của thanh toán cần xóa.</param>
        /// <returns>Một IActionResult cho biết trạng thái của thao tác xóa.</returns>
        [HttpDelete("{paymentId}")]
        public async Task<IActionResult> DeletePayment(int paymentId)
        {
            // Tương tự như GetPaymentById, bạn có thể muốn thêm logic kiểm tra quyền xóa.

            var paymentToDelete = await _paymentService.GetPaymentById(paymentId);
            if (paymentToDelete == null)
            {
                return NotFound();
            }

            // Lấy UserId từ token để kiểm tra quyền sở hữu
            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            int currentUserId = TokenHelper.GetUserIdFromToken(authorizationHeader);

            // Kiểm tra xem người dùng hiện tại có phải là chủ sở hữu của thanh toán này hay không
            if (paymentToDelete.UserId != currentUserId && !User.IsInRole("Admin")) // Ví dụ: cho phép admin xóa mọi thứ
            {
                return Forbid(); // Trả về 403 Forbidden nếu không có quyền
            }

            var deleted = await _paymentService.DeletePayment(paymentId);
            if (deleted)
            {
                return NoContent(); // Trả về 204 No Content nếu xóa thành công
            }
            return StatusCode(500, "Đã xảy ra lỗi khi xóa thanh toán."); // Trả về 500 nếu có lỗi server
        }

        // Bạn có thể thêm các action khác tại đây, ví dụ như tạo mới thanh toán (POST), cập nhật thông tin thanh toán (PUT), v.v.
    }
}