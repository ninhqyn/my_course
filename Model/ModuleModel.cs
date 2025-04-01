namespace MyCourse.Model
{
    public class ModuleModel
    {
        public int ModuleId { get; set; }

        public int CourseId { get; set; }

        public string ModuleName { get; set; } = null!;

        public string? Description { get; set; }

        public int OrderIndex { get; set; }

        public int? DurationMinutes { get; set; }

        public bool? IsFree { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? LessonCount { get; set; }

        public List<LessonModel> Lessons { get; set; }
    }
}
