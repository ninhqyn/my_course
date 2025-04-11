using AutoMapper;
using MyCourse.Data;
using MyCourse.IServices;
using MyCourse.Model;
using Microsoft.EntityFrameworkCore;

namespace MyCourse.Services
{
    public class InstructorService : IInstructorService
    {
        private readonly MyCourseContext _context;
        private readonly IMapper _mapper;

        public InstructorService(MyCourseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

    
        public async Task<List<InstructorModel>> GetAllInstructorsByCourseIdAsync(int courseId)
        {
         
            var courseInstructors = await _context.CourseInstructors
                                                   .Where(ci => ci.CourseId == courseId)
                                                   .Include(ci => ci.Instructor)
                                                   .ToListAsync();

          
            var instructors = courseInstructors
                              .Select(ci => ci.Instructor)
                              .ToList();

            var instructorModels = _mapper.Map<List<InstructorModel>>(instructors);

            return instructorModels;
        }

        public async Task<InstructorModel> GetInstructorByIdAsync(int instructorId)
        {
            var instructor = await _context.Instructors.FindAsync(instructorId);
            if (instructor != null) {
                return _mapper.Map<InstructorModel>(instructor);
            }
            return null;
        }
    }
}
