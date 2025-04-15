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

        public async Task<QuizResultModel> AddQuizResult(QuizResultRequest quizResultRequest)
        {
            try
            {
                // Get the quiz with questions and correct answers
                var quiz = await _context.Quizzes
                    .Where(q => q.QuizId == quizResultRequest.QuizId)
                    .Include(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync();

                if (quiz == null)
                {
                    return new QuizResultModel
                    {
                        Success = false,
                        Message = $"Quiz with ID {quizResultRequest.QuizId} not found."
                    };
                }

                // Calculate score
                decimal totalPoints = quiz.Questions.Sum(q => q.Points ?? 0);
                decimal earnedPoints = 0;
                int correctAnswers = 0;

                // Track correct answers
                foreach (var userAnswer in quizResultRequest.Answers)
                {
                    var question = quiz.Questions.FirstOrDefault(q => q.QuestionId == userAnswer.QuestionId);
                    if (question != null)
                    {
                        var selectedAnswer = question.Answers.FirstOrDefault(a => a.AnswerId == userAnswer.AnswerId);
                        if (selectedAnswer != null && selectedAnswer.IsCorrect == true)
                        {
                            earnedPoints += question.Points ?? 0;
                            correctAnswers++;
                        }
                    }
                }

                // Calculate final score as percentage
                decimal scorePercentage = totalPoints > 0 ? (earnedPoints / totalPoints) * 100 : 0;

                // Determine if passed (assuming 70% is passing score, adjust as needed)
                bool passed = scorePercentage >= 70;

                // Get attempt number
                int attemptNumber = 1;
                var previousAttempts = await _context.QuizResults
                    .Where(r => r.QuizId == quizResultRequest.QuizId && r.UserId == quizResultRequest.UserId)
                    .OrderByDescending(r => r.AttemptNumber)
                    .FirstOrDefaultAsync();

                if (previousAttempts != null && previousAttempts.AttemptNumber.HasValue)
                {
                    attemptNumber = previousAttempts.AttemptNumber.Value + 1;
                }

                // Create quiz result record
                var quizResult = new QuizResult
                {
                    QuizId = quizResultRequest.QuizId,
                    UserId = quizResultRequest.UserId,
                    Score = scorePercentage,
                    Passed = passed,
                    TimeSpentMinutes = quizResultRequest.TimeSpentMinutes,
                    AttemptNumber = attemptNumber,
                    SubmissionDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                // Add to database
                await _context.QuizResults.AddAsync(quizResult);
                await _context.SaveChangesAsync();

                // Create and return the result model
                return new QuizResultModel
                {
                    ResultId = quizResult.ResultId,
                    Score = scorePercentage,
                    Passed = passed,
                    TotalQuestions = quiz.Questions.Count,
                    CorrectAnswers = correctAnswers,
                    AttemptNumber = attemptNumber,
                    SubmissionDate = quizResult.SubmissionDate ?? DateTime.UtcNow,
                    Success = true,
                    Message = passed ? "Congratulations! You passed the quiz." : "You did not pass the quiz. You can try again."
                };
            }
            catch (Exception ex)
            {
                // Log exception if needed
                return new QuizResultModel
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<List<QuizResultModel>> GetAllQuizResultByQuizIdAndUserId(int quizId, int userId)
        {
            try
            {
                // Lấy tất cả kết quả bài kiểm tra của người dùng cho quiz đã cho
                var quizResults = await _context.QuizResults
                    .Where(r => r.QuizId == quizId && r.UserId == userId)
                    .OrderByDescending(r => r.SubmissionDate) // Sắp xếp theo thời gian nộp từ mới đến cũ
                    .ToListAsync();

                if (!quizResults.Any())
                {
                    // Trả về danh sách rỗng nếu không có kết quả nào
                    return new List<QuizResultModel>();
                }

                // Lấy thông tin về quiz để có số lượng câu hỏi
                var quiz = await _context.Quizzes
                    .Where(q => q.QuizId == quizId)
                    .Include(q => q.Questions)
                    .FirstOrDefaultAsync();

                int totalQuestions = quiz?.Questions.Count ?? 0;

                // Tạo danh sách kết quả
                var resultModels = new List<QuizResultModel>();

                foreach (var result in quizResults)
                {
                    // Ước tính số câu trả lời đúng dựa trên điểm số
                    int estimatedCorrectAnswers = totalQuestions > 0
                        ? (int)Math.Round(totalQuestions * (result.Score / 100m))
                        : 0;

                    // Tạo và thêm mô hình kết quả vào danh sách
                    resultModels.Add(new QuizResultModel
                    {
                        ResultId = result.ResultId,
                        Score = result.Score,
                        Passed = result.Passed ?? false,
                        TotalQuestions = totalQuestions,
                        CorrectAnswers = estimatedCorrectAnswers,
                        AttemptNumber = result.AttemptNumber ?? 10,
                        SubmissionDate = result.SubmissionDate ?? DateTime.UtcNow,
                        Success = true,
                        Message = result.Passed == true
                            ? "Đã vượt qua bài kiểm tra."
                            : "Chưa vượt qua bài kiểm tra."
                    });
                }

                return resultModels;
            }
            catch (Exception ex)
            {
                // Ghi log ngoại lệ nếu cần
                // Trả về danh sách chứa một phần tử thông báo lỗi
                return new List<QuizResultModel>
        {
            new QuizResultModel
            {
                Success = false,
                Message = $"Đã xảy ra lỗi khi lấy kết quả: {ex.Message}"
            }
        };
            }
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
