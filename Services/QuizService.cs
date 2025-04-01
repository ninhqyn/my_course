using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyCourse.Data;
using MyCourse.IServices;
using MyCourse.Model;

namespace MyCourse.Services
{
    public class QuizService : IQuizService
    {
        private readonly MyCourseContext _context;
        private readonly IMapper _mapper;

        // Constructor to inject dependencies
        public QuizService(MyCourseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Method to get quizzes by course ID, including answers for each question
        public async Task<List<QuizModel>> GetQuizByCourseIdAsync(int courseId)
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.Module.CourseId == courseId) // Filter quizzes by courseId
                .Include(q => q.Questions) // Include questions for each quiz
                .ThenInclude(q => q.Answers) // Include answers for each question
                .ToListAsync();

            // Map to QuizModel and return
            return _mapper.Map<List<QuizModel>>(quizzes);
        }

        // Method to get a quiz by its ID, including answers for each question
        public async Task<QuizModel> GetQuizByIdAsync(int quizId)
        {
            var quiz = await _context.Quizzes
                .Where(q => q.QuizId == quizId)
                .Include(q => q.Questions) // Include questions for the quiz
                .ThenInclude(q => q.Answers) // Include answers for each question
                .FirstOrDefaultAsync();

            if (quiz == null)
            {
                throw new KeyNotFoundException($"Quiz with ID {quizId} not found.");
            }

            return _mapper.Map<QuizModel>(quiz); // Map the entity to QuizModel
        }
    }
}
