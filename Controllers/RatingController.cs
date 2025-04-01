using Microsoft.AspNetCore.Mvc;
using MyCourse.IServices;
using MyCourse.Model;

namespace MyCourse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService _ratingService;

        public RatingController(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        // GET: api/rating/course/{courseId}
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<List<RatingModel>>> GetRatingsByCourseId(int courseId, int page = 1, int pageSize = 10)
        {
            var ratings = await _ratingService.GetRatingByCourseId(courseId, page, pageSize);
            if (ratings == null || ratings.Count == 0)
            {
                return NotFound("No ratings found for this course.");
            }

            return Ok(ratings);
        }

        // GET: api/rating/total/{courseId}
        [HttpGet("total/{courseId}")]
        public async Task<ActionResult<RatingTotalMoel>> GetRatingTotalByCourseId(int courseId)
        {
            var ratingTotal = await _ratingService.GetRatingTotalByCourseId(courseId);
            return Ok(ratingTotal);
        }
    }
}
