namespace MyCourse.Model
{
    public class LessonModel
    {
        public int LessonId { get; set; }

        public int ModuleId { get; set; }

        public string LessonName { get; set; } = null!;

        public string? Content { get; set; }

        public string? VideoUrl { get; set; }

        public int? DurationMinutes { get; set; }

        public int OrderIndex { get; set; }

        public bool? IsPreview { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
