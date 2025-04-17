using AutoMapper;
using MyCourse.Data;
using MyCourse.Model;

namespace MyCourse.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Map từ Course sang CourseModel
            CreateMap<Course, CourseModel>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
             
            CreateMap<Skill, SkillModel>();
            // Map từ Category sang CategoryModel
            CreateMap<Category, CategoryModel>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            CreateMap<Instructor, InstructorModel>();

            CreateMap<Module, ModuleModel>();

            CreateMap<Lesson, LessonModel>();

            CreateMap<Rating, RatingModel>();


            CreateMap<Enrollment, UserCourse>()
    .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Course.CourseId))
    .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Course.Description))
    .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => src.Course.ThumbnailUrl))
    .ForMember(dest => dest.DifficultyLevel, opt => opt.MapFrom(src => src.Course.DifficultyLevel))
    .ForMember(dest => dest.IsFeatured, opt => opt.MapFrom(src => src.Course.IsFeatured))
    .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Course.IsActive))
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Course.CreatedAt))
    .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.Course.UpdatedAt))
    .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Course.Category))
    .ForMember(dest => dest.Progress, opt => opt.MapFrom(src => src.ProgressPercentage));

            CreateMap<User,UserModel>();
            CreateMap<Quiz, QuizModel>();
            CreateMap<Question, QuestionModel>();
            CreateMap<Answer, AnswerModel>();
            CreateMap<LessonProgress, LessonProgressModel>();
            CreateMap<Cart, CartModel>();
        }
    }
}