namespace MyCourse.Model
{
    public class CreateProgressModel
    {
    
        public int LessonId { get; set; }

        public int CourseId { get; set; }
        public bool? IsCompleted { get; set; }

        public int? LastPositionSeconds { get; set; }

        public int? TimeSpentMinutes { get; set; }

       
    }
}
