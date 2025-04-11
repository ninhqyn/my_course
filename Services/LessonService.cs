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
        public async Task<LessonProgressModel> CreateOrUpdateLessonProgressAsync(int userId, CreateProgressModel createProgress)
        {
            // Kiểm tra xem bản ghi tiến độ đã tồn tại chưa
            var existingProgress = await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.LessonId == createProgress.LessonId && lp.UserId == userId);

            if (existingProgress == null)
            {
                // Tạo bản ghi tiến độ mới
                var newProgress = new LessonProgress
                {
                    LessonId = createProgress.LessonId,
                    UserId = userId,
                    IsCompleted = createProgress.IsCompleted ?? false,
                    LastPositionSeconds = createProgress.LastPositionSeconds,
                    TimeSpentMinutes = createProgress.TimeSpentMinutes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Nếu bài học đã hoàn thành, cập nhật ngày hoàn thành
                if (newProgress.IsCompleted == true)
                {
                    newProgress.CompletionDate = DateTime.UtcNow;
                }

                _context.LessonProgresses.Add(newProgress);
                await _context.SaveChangesAsync();

                // Cập nhật thông tin Enrollment
                await UpdateEnrollmentProgressAsync(userId, createProgress.CourseId);

                return _mapper.Map<LessonProgressModel>(newProgress);
            }
            else
            {
                // Cập nhật bản ghi tiến độ hiện có
                existingProgress.LastPositionSeconds = createProgress.LastPositionSeconds ?? existingProgress.LastPositionSeconds;
                existingProgress.TimeSpentMinutes = createProgress.TimeSpentMinutes ?? existingProgress.TimeSpentMinutes;
                existingProgress.UpdatedAt = DateTime.UtcNow;

                // Nếu IsCompleted được đặt thành true
                if (createProgress.IsCompleted == true && existingProgress.IsCompleted != true)
                {
                    existingProgress.IsCompleted = true;
                    existingProgress.CompletionDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                // Cập nhật thông tin Enrollment
                await UpdateEnrollmentProgressAsync(userId, createProgress.CourseId);

                return _mapper.Map<LessonProgressModel>(existingProgress);
            }
        }

        // Phương thức để cập nhật tiến độ của Enrollment
        private async Task UpdateEnrollmentProgressAsync(int userId, int courseId)
        {
            // Tìm hoặc tạo enrollment
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment == null)
            {
                // Tạo enrollment mới nếu chưa tồn tại
                enrollment = new Enrollment
                {
                    UserId = userId,
                    CourseId = courseId,
                    EnrollmentDate = DateTime.UtcNow,
                    IsActive = true,
                    ProgressPercentage = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Enrollments.Add(enrollment);
            }

            // Tính toán tiến độ phần trăm của khóa học
            decimal progressPercentage = await CalculateCourseProgressPercentageAsync(userId, courseId);

            // Cập nhật tiến độ
            enrollment.ProgressPercentage = progressPercentage;
            enrollment.UpdatedAt = DateTime.UtcNow;

            // Nếu tiến độ đạt 100%, đánh dấu là hoàn thành
            if (progressPercentage >= 100)
            {
                enrollment.CompletionDate = enrollment.CompletionDate ?? DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // Phương thức tính toán phần trăm tiến độ khóa học
        private async Task<decimal> CalculateCourseProgressPercentageAsync(int userId, int courseId)
        {
            // Lấy tất cả các bài học trong khóa học
            var lessonsInCourse = await _context.Lessons
                .Include(l => l.Module)
                .Where(l => l.Module.CourseId == courseId)
                .ToListAsync();

            if (!lessonsInCourse.Any())
                return 0;

            // Lấy tiến độ của các bài học đã hoàn thành
            var completedLessons = await _context.LessonProgresses
                .Where(lp => lp.UserId == userId && lessonsInCourse.Select(l => l.LessonId).Contains(lp.LessonId) && lp.IsCompleted == true)
                .ToListAsync();

            // Tính phần trăm hoàn thành
            decimal totalLessons = lessonsInCourse.Count;
            decimal completedCount = completedLessons.Count;

            return Math.Round((completedCount / totalLessons) * 100, 2);
        }
    }
}
