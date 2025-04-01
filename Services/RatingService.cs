using AutoMapper;
using MyCourse.Data;
using MyCourse.IServices;
using MyCourse.Model;
using Microsoft.EntityFrameworkCore;

namespace MyCourse.Services
{
    public class RatingService : IRatingService
    {
        private readonly MyCourseContext _context;
        private readonly IMapper _mapper;

        public RatingService(MyCourseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Method to get ratings for a course with pagination
        public async Task<List<RatingModel>> GetRatingByCourseId(int courseId, int page = 1, int pageSize = 10)
        {
            // Fetch ratings for the course with pagination
            var ratings = await _context.Ratings
                .Where(r => r.CourseId == courseId && r.IsApproved == true) // Only approved ratings
                .OrderByDescending(r => r.CreatedAt) // Optional: Order by creation date
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(u=>u.User)
                .ToListAsync();

            // Map to RatingModel
            var ratingModels = _mapper.Map<List<RatingModel>>(ratings);

            return ratingModels;
        }

        // Method to get the total rating (average rating and total count) for a course
        public async Task<RatingTotalMoel> GetRatingTotalByCourseId(int courseId)
        {
            // Fetch ratings for the course
            var ratings = await _context.Ratings
                .Where(r => r.CourseId == courseId && r.IsApproved == true) // Only approved ratings
                .ToListAsync();

            // Calculate the average rating and total count
            double averageRating = ratings.Any() ? ratings.Average(r => r.RatingValue) : 0;
            int totalRating = ratings.Count;

            // Return the RatingTotalMoel
            return new RatingTotalMoel
            {
                courseId = courseId,
                ratingValue = averageRating,
                totalRating = totalRating
            };
        }
    }
}
