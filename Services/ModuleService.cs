using AutoMapper;
using MyCourse.IServices;
using MyCourse.Model;
using MyCourse.Data;
using Microsoft.EntityFrameworkCore;
namespace MyCourse.Services
{
    public class ModuleService : IModuleService
    {
        private readonly MyCourseContext _context;
        private readonly IMapper _mapper;
        public ModuleService(MyCourseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<List<ModuleModel>> GetAllModulesByCourseIdAsync(int courseId)
        {
            var modules = await _context.Modules
                .Where(m => m.CourseId == courseId)
                .Include(l=> l.Lessons)
                .ToListAsync();
            return _mapper.Map<List<ModuleModel>>(modules);

        }



        public async Task<List<UserModule>> GetAllUserModuleByCourseIdAsync(int courseId, int userId)
        {
            var modules = await _context.Modules
                .Where(m => m.CourseId == courseId)
                .Include(m => m.Lessons)
                .ToListAsync();

            var lessonProgress = await _context.LessonProgresses
                .Where(lp => lp.UserId == userId &&
                            lp.Lesson.Module.CourseId == courseId)
                .ToListAsync();

            // Create a dictionary for quick lookup of lesson completion status
            var completionStatus = lessonProgress.ToDictionary(
                lp => lp.LessonId,
                lp => lp.IsCompleted ?? false
            );

            // Map to UserModule list with completion status
            var userModules = new List<UserModule>();

            foreach (var module in modules)
            {
                var userLessons = module.Lessons.Select(lesson => new UserLesson
                {
                    LessonId = lesson.LessonId,
                    ModuleId = lesson.ModuleId,
                    LessonName = lesson.LessonName,
                    Content = lesson.Content,
                    VideoUrl = lesson.VideoUrl,
                    DurationMinutes = lesson.DurationMinutes,
                    OrderIndex = lesson.OrderIndex,
                    IsPreview = lesson.IsPreview,
                    // Check if lesson progress exists, if not default to false
                    IsCompleted = completionStatus.ContainsKey(lesson.LessonId)
                        ? completionStatus[lesson.LessonId]
                        : false,
                    CreatedAt = lesson.CreatedAt,
                    UpdatedAt = lesson.UpdatedAt
                }).ToList();

                userModules.Add(new UserModule
                {
                    ModuleId = module.ModuleId,
                    CourseId = module.CourseId,
                    ModuleName = module.ModuleName,
                    Description = module.Description,
                    OrderIndex = module.OrderIndex,
                    DurationMinutes = module.DurationMinutes,
                    IsFree = module.IsFree,
                    CreatedAt = module.CreatedAt,
                    UpdatedAt = module.UpdatedAt,
                    LessonCount = module.Lessons.Count,
                    Lessons = userLessons
                });
            }

            return userModules;
        
        }
    }
}
