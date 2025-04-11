using Microsoft.AspNetCore.Mvc;
using MyCourse.IServices;
using MyCourse.Model;

namespace MyCourse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstructorController : ControllerBase
    {
        private readonly IInstructorService _instructorService;

       
        public InstructorController(IInstructorService instructorService)
        {
            _instructorService = instructorService;
        }

     
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetInstructorsByCourseId(int courseId)
        {
            try
            {
                var instructors = await _instructorService.GetAllInstructorsByCourseIdAsync(courseId);

                if (instructors == null || instructors.Count == 0)
                {
                    return NotFound(new { message = "Không có giảng viên nào cho khóa học này." });
                }

                return Ok(instructors); 
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, new { message = "Có lỗi xảy ra trong quá trình lấy giảng viên.", details = ex.Message });
            }
        }

        [HttpGet("{instructorId}")]
        public async Task<IActionResult> GetInstructorById(int instructorId)
        {
            try
            {
                var instructor = await _instructorService.GetInstructorByIdAsync(instructorId);

                if (instructor == null)
                {
                    return NotFound(new { message = "Không tìm thấy giảng viên với ID này." });
                }

                return Ok(instructor);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra trong quá trình lấy giảng viên.", details = ex.Message });
            }
        }
    }
}
