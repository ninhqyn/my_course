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

                // Determine if passed (assuming 80% is passing score, adjust as needed)
                bool passed = scorePercentage >= 80;

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

                //
                string message = passed ? "Congratulations! You passed the quiz." : "You did not pass the quiz. You can try again.";
                bool certificateCreated = false;
                int? certificateId = null;

                // Kiểm tra nếu là final quiz và passed
                if (quiz.IsFinal == true && passed)
                {
                    var courseId = quiz.Module.CourseId;

                    // Cập nhật enrollment completed
                    await UpdateEnrollmentCompletedAsync(courseId, quizResultRequest.UserId);

                    // Kiểm tra xem đã có certificate chưa
                    var existingCertificate = await _context.Certificates
                        .FirstOrDefaultAsync(c => c.CourseId == courseId && c.UserId == quizResultRequest.UserId);

                    if (existingCertificate == null)
                    {
                        // Tạo certificate mới
                        var certificate = new Certificate
                        {
                            UserId = quizResultRequest.UserId,
                            CourseId = courseId,
                            IssueDate = DateTime.UtcNow,
                            VerificationCode = GenerateVerificationCode(),
                            IsValid = true,
                            CreatedAt = DateTime.UtcNow,
                            CertificateUrl = GenerateCertificateUrl(courseId, quizResultRequest.UserId)
                        };

                        _context.Certificates.Add(certificate);
                        await _context.SaveChangesAsync();

                        certificateCreated = true;
                        certificateId = certificate.CertificateId;
                        message = "Chúc mừng! Bạn đã hoàn thành khóa học và nhận được chứng chỉ.";
                    }
                    else
                    {
                        message = "Bạn đã hoàn thành bài kiểm tra cuối khóa thành công.";
                    }
                }

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
                    Message = message,
                    CertificateId = certificateId,
                    CertificateCreated = certificateCreated
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
        private async Task UpdateEnrollmentCompletedAsync(int courseId, int userId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId);

            if (enrollment != null)
            {
               
                enrollment.CompletionDate = DateTime.UtcNow; // Nếu có field này
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateVerificationCode()
        {
            return Guid.NewGuid().ToString("N")[..12].ToUpper(); // 12 ký tự
        }

        private string GenerateCertificateUrl(int courseId, int userId)
        {
            return $"/certificates/{courseId}/{userId}/{DateTime.UtcNow:yyyyMMdd}";
        }
        public async Task<bool> CanTakeFinalQuizAsync(int courseId, int userId)
        {
            try
            {
                // 1. Kiểm tra Enrollment - ProgressPercentage phải là 100%
                var enrollment = await _context.Enrollments
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

                if (enrollment == null || enrollment.ProgressPercentage != 100)
                {
                    return false;
                }

                // 2. Lấy tất cả Quiz của course (trừ final quiz)
                var quizzes = await _context.Quizzes
                    .Where(q => q.Module.CourseId == courseId && q.IsFinal != true)
                    .ToListAsync();

                if (!quizzes.Any())
                {
                    return true; // Nếu không có quiz nào thì cho phép làm final quiz
                }

                // 3. Kiểm tra từng quiz xem có kết quả Passed không
                foreach (var quiz in quizzes)
                {
                    var passedResult = await _context.QuizResults
                        .Where(qr => qr.QuizId == quiz.QuizId && qr.UserId == userId && qr.Passed == true)
                        .FirstOrDefaultAsync();

                    // Nếu chưa làm quiz này hoặc chưa có kết quả Passed
                    if (passedResult == null)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log exception nếu cần
                // _logger.LogError(ex, "Error checking if user can take final quiz");
                return false;
            }
        }

        public async Task<List<QuizResultModel>> GetAllQuizResultByQuizIdAndUserId(int quizId, int userId, int page = 1, int pageSize = 10)
        {
            try
            {
                // Validate input parameters
                if (page <= 0 || pageSize <= 0)
                {
                    throw new ArgumentException("Page and pageSize must be greater than zero.");
                }

                // Lấy thông tin về quiz để có số lượng câu hỏi. Chỉ lấy một lần để tránh lặp lại truy vấn.
                var quiz = await _context.Quizzes
              .Where(q => q.QuizId == quizId)
              .Include(q => q.Questions)
              .FirstOrDefaultAsync();

                if (quiz == null)
                {
                    return new List<QuizResultModel>(); // Trả về danh sách rỗng nếu không tìm thấy quiz
                }
                int totalQuestions = quiz.Questions.Count;

                // Lấy tất cả kết quả bài kiểm tra của người dùng cho quiz đã cho, có phân trang
                var quizResults = await _context.QuizResults
              .Where(r => r.QuizId == quizId && r.UserId == userId)
              .OrderByDescending(r => r.SubmissionDate) // Sắp xếp theo thời gian nộp từ mới đến cũ
                    .Skip((page - 1) * pageSize) // Thực hiện phân trang
                    .Take(pageSize)
              .ToListAsync();

                if (!quizResults.Any())
                {
                    return new List<QuizResultModel>(); // trả về luôn để response là 200 OK
                }

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
                // Ghi log ngoại lệ nếu cần (sử dụng logging framework như Serilog)
                Console.Error.WriteLine($"Error in GetAllQuizResultByQuizIdAndUserId: {ex}");
                return new List<QuizResultModel>(); // Trả về danh sách rỗng
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
