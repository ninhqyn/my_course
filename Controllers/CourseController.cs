using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCourse.IServices;
using MyCourse.Model;

namespace MyCourse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        // GET: api/Course
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseModel>>> GetCourses()
        {
            var courses = await _courseService.GetAllCourse();
            return Ok(courses);
        }

        // GET: api/Course/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CourseModel>> GetCourse(int id)
        {
            var course = await _courseService.GetCourse(id);

            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }

        // POST: api/Course
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CourseModel>> CreateCourse([FromBody] CourseModel courseModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdCourse = await _courseService.CreateCourse(courseModel);
                return CreatedAtAction(nameof(GetCourse), new { id = createdCourse.CourseId }, createdCourse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Course/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] CourseModel courseModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != courseModel.CourseId)
            {
                return BadRequest("ID trong đường dẫn không khớp với ID trong dữ liệu.");
            }

            var updatedCourse = await _courseService.UpdateCourse(id, courseModel);

            if (updatedCourse == null)
            {
                return NotFound();
            }

            return Ok(updatedCourse);
        }

        // DELETE: api/Course/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var result = await _courseService.DeleteCourse(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // GET: api/Course/category/{categoryId}
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CourseModel>>> GetCoursesByCategoryId(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var courses = await _courseService.GetAllCourseByCategoryId(categoryId, page, pageSize);
                if (courses == null || courses.Count == 0)
                {
                    return NotFound();
                }

                return Ok(courses);
            }
            catch (Exception e) {
                return BadRequest(e);
            }
            
        }
       

        // GET: api/Course/new
        [HttpGet("new")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseModel>>> GetNewCourses([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var courses = await _courseService.GetAllNewCourse(page, pageSize);
            return Ok(courses);
        }
        // GET: api/Course/filter
        [HttpGet("filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseModel>>> GetCoursesByFilter([FromQuery] string keyword = "", [FromQuery] int categoryId = 0, [FromQuery] int instructorId = 0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var courses = await _courseService.GetAllCourseByFilter(keyword, categoryId, instructorId, page, pageSize);
            return Ok(courses);
        }
        // GET: api/Course/favorites
        [HttpGet("favorites")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CourseModel>>> GetFavoriteCourses([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var courses = await _courseService.GetAllFavoriteCourse(page, pageSize);
            return Ok(courses);
        }

        [HttpGet("user-courses")]
        [Authorize] // Make sure the user is authenticated
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<UserCourse>>> GetCoursesByUserId()
        {
            // Extract the userId from the token
            var userId = TokenHelper.GetUserIdFromToken(Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""));

            if (userId == 0)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            // Fetch the courses the user has enrolled in
            var courses = await _courseService.GetAllCourseByUserId(userId);

            return Ok(courses);
        }

       




    }
}