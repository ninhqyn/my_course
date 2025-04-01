namespace MyCourse.Model
{
    public class UserCourse
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; } = null!;

        public string? Description { get; set; }

        public string? ThumbnailUrl { get; set; }

        public string? DifficultyLevel { get; set; }

        public bool? IsFeatured { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public CategoryModel Category { get; set; }

        public decimal? Progress { get; set; }
    }
}
