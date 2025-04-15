using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCourse.IServices;
using MyCourse.Model;

namespace MyCourse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;

        
        public QuizController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        // GET: api/Quiz/{courseId}
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetQuizzesByCourseIdAsync(int courseId)
        {
            try
            {
                var quizzes = await _quizService.GetQuizByCourseIdAsync(courseId);
                return Ok(quizzes); // Return a 200 OK response with the quizzes
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Quiz/{quizId}
        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuizByIdAsync(int quizId)
        {
            try
            {
                var quiz = await _quizService.GetQuizByIdAsync(quizId);
                return quiz == null ? NotFound() : Ok(quiz); // Return 404 if quiz is not found, otherwise 200 OK
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Quiz with ID {quizId} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Quiz/quiz-result
        [HttpPost("quiz-result")]
        [Authorize]
        public async Task<IActionResult> AddQuizResult([FromBody] QuizResultRequest quizResultRequest)
        {
            try
            {
                var userId = TokenHelper.GetUserIdFromToken(Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""));

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Token không hợp lệ" });
                }
                quizResultRequest.UserId = userId;
                var result = await _quizService.AddQuizResult(quizResultRequest);
                if (result.Success)
                {
                
                    return Ok(result);
                }
                else
                {
                
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetAll/quiz-result/{quizId}")]
        [Authorize]
        public async Task<IActionResult> GetAllQuizResult(int quizId)
        {
            try
            {
                // Lấy userId từ token xác thực
                var userId = TokenHelper.GetUserIdFromToken(Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""));
                if (userId == 0)
                {
                    return Unauthorized(new { message = "Token không hợp lệ" });
                }

                // Gọi service để lấy tất cả kết quả quiz
                var results = await _quizService.GetAllQuizResultByQuizIdAndUserId(quizId, userId);

                // Kiểm tra xem có lỗi trong kết quả trả về không
                if (results != null && results.Count > 0)
                {
                    // Kiểm tra nếu kết quả đầu tiên có thông báo lỗi
                    var firstResult = results.FirstOrDefault();
                    if (firstResult != null && firstResult.Success == false)
                    {
                        return BadRequest(new { message = firstResult.Message });
                    }

                    // Trả về danh sách kết quả nếu không có lỗi
                    return Ok(new
                    {
                        success = true,
                        data = results,
                        message = "Lấy danh sách kết quả quiz thành công."
                    });
                }
                else
                {
                    // Trường hợp không có kết quả nào
                    return Ok(new
                    {
                        success = true,
                        data = results,
                        message = "Không tìm thấy kết quả nào cho quiz này."
                    });
                }
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và trả về lỗi 500
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Lỗi máy chủ: {ex.Message}"
                });
            }
        }
    
    }
}
