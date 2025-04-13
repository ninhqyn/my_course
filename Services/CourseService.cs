using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyCourse.Data;
using MyCourse.IServices;
using MyCourse.Model;
using System.Linq;

namespace MyCourse.Services
{
    public class CourseService : ICourseService
    {
        private readonly MyCourseContext _context;
        private readonly IMapper _mapper;
        public CourseService(MyCourseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CourseModel>> GetAllCourse()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
               
                .ToListAsync();
            return _mapper.Map<List<CourseModel>>(courses);
        }

        public async Task<CourseModel> GetCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
               
                .FirstOrDefaultAsync(c => c.CourseId == id);
            if (course == null)
            {
                return null;
            }
            return _mapper.Map<CourseModel>(course);
        }

        public async Task<CourseModel> CreateCourse(CourseModel courseModel)
        {
            var course = _mapper.Map<Course>(courseModel);
            course.CreatedAt = DateTime.Now;
            course.UpdatedAt = DateTime.Now;
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            // Tải lại course với category để trả về model đầy đủ
            await _context.Entry(course).Reference(c => c.Category).LoadAsync();
          
            return _mapper.Map<CourseModel>(course);
        }

        public async Task<CourseModel> UpdateCourse(int id, CourseModel courseModel)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return null;
            }
            // Cập nhật các thuộc tính
            _mapper.Map(courseModel, course);
            course.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            // Tải lại course với category và instructor
            await _context.Entry(course).Reference(c => c.Category).LoadAsync();
          
            return _mapper.Map<CourseModel>(course);
        }

        public async Task<bool> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return false;
            }
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CourseModel>> GetAllCourseByCategoryId(int categoryId, int page = 1, int pageSize = 10)
        {
            var courses = await _context.Courses
                .Where(c => c.CategoryId == categoryId)
                .Include(c => c.Category)
               
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<List<CourseModel>>(courses);
        }

        

        public async Task<List<CourseModel>> GetAllFavoriteCourse(int page = 1, int pageSize = 10)
        {
            // Lấy các khóa học đã đánh dấu là yêu thích và đang hoạt động
            var courses = await _context.Courses
                .Where(c => c.IsFeatured == true && c.IsActive == true)
                .Include(c => c.Category)
               
                .Include(r => r.Ratings)
                // Sắp xếp theo điểm đánh giá trung bình của tất cả người dùng
                .OrderByDescending(c => c.Ratings.Count > 0
                    ? c.Ratings.Average(r => r.RatingValue)
                    : 0)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<List<CourseModel>>(courses);
        }

        public async Task<List<CourseModel>> GetAllNewCourse(int page = 1, int pageSize = 10)
        {
            var courses = await _context.Courses
                .Where(c => c.IsActive == true)
                .Include(c => c.Category)
                
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<List<CourseModel>>(courses);
        }

        public async Task<List<CourseModel>> GetAllCourseByFilter(string keyword = "", int categoryId = 0, int instructorId = 0, int page = 1, int pageSize = 10)
        {
            var query = _context.Courses.AsQueryable();

            // Lọc theo từ khóa
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c => c.CourseName.Contains(keyword) ||
                                        c.Description.Contains(keyword));
            }

            // Lọc theo danh mục
            if (categoryId > 0)
            {
                query = query.Where(c => c.CategoryId == categoryId);
            }


            // Chỉ lấy các khóa học đang hoạt động
            query = query.Where(c => c.IsActive == true);

            // Thực hiện truy vấn
            var courses = await query
                .Include(c => c.Category)
              
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return _mapper.Map<List<CourseModel>>(courses);
        }

        public async Task<List<UserCourse>> GetAllCourseByUserId(int userId)
        {
            // Lấy danh sách các đăng ký khóa học của người dùng
            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == userId && e.IsActive == true)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .ToListAsync();

            return _mapper.Map<List<UserCourse>>(enrollments);
        }

        public async Task<List<CourseModel>> GetAllCourseByInstructorId(int instructorId)
        {
            try
            {
                // Validate parameter
                if (instructorId <= 0)
                {
                    throw new ArgumentException("InstructorId không hợp lệ");
                }

                // Query courses through CourseInstructor table
                var courses = await _context.CourseInstructors
                    .Include(c => c.Course)
                    .ThenInclude(ca=>ca.Category)
                    .Where(ci => ci.InstructorId == instructorId)
                    .Select(ci => ci.Course)
                    .Where(c => c.IsActive == true)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                // Map to CourseModel
                var courseModels = _mapper.Map<List<CourseModel>>(courses);

                return courseModels;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách khóa học của instructor: {ex.Message}");
            }
        }




        public async Task<bool> IsEnrollmentCourse(int userId, int courseId)
        {
         
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e =>
                    e.UserId == userId &&
                    e.CourseId == courseId &&
                    e.IsActive == true);
            return enrollment != null;
        }

    }

}