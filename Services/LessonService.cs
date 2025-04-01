using AutoMapper;
using MyCourse.Data;
using MyCourse.IServices;
using MyCourse.Model;
using Microsoft.EntityFrameworkCore;

namespace MyCourse.Services
{
    public class LessonService : ILessonService
    {
        private readonly MyCourseContext _context;
        private readonly IMapper _mapper;

        public LessonService(MyCourseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        

        public async Task<List<LessonModel>> GetLessonByModuleIdAsync(int moduleId)
        {
            
            var lessons = await _context.Lessons
                                        .Where(lesson => lesson.ModuleId == moduleId)
                                        .ToListAsync();
            return _mapper.Map<List<LessonModel>>(lessons);
        }

      
        public async Task<LessonModel> GetLessonModuleById(int lessonId)
        {
            
            var lesson = await _context.Lessons
                                        .Include(l => l.Module) 
                                        .FirstOrDefaultAsync(l => l.LessonId == lessonId);
            if (lesson == null)
            {
                return null;
            }

        
            return _mapper.Map<LessonModel>(lesson);
        }

        public Task<LessonProgressModel> GetLessonProgressByLessonIdAndUserId(int lessonId, int userId)
        {
            throw new NotImplementedException();
        }

        public Task<List<LessonProgressModel>> GetLessonProgressByUserId(int userId, int courseId)
        {
            throw new NotImplementedException();
        }
        public Task<LessonProgressModel> CreateLessonProgressAsync(CreateProgressModel createProgress)
        {
            throw new NotImplementedException();
        }
    }
}
