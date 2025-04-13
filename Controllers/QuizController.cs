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
    }
}
