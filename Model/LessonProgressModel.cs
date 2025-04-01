namespace MyCourse.Model
{
    public class LessonProgressModel
    {
        public int ProgressId { get; set; }

        public LessonModel Lesson { get; set; }

        public int UserId { get; set; }

        public bool? IsCompleted { get; set; }

        public int? LastPositionSeconds { get; set; }

        public int? TimeSpentMinutes { get; set; }

        public DateTime? CompletionDate { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
