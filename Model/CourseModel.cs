namespace MyCourse.Model
{
    public class CourseModel
    {
        public int CourseId { get; set; }

        public int? TemplateId { get; set; }

     
        public string CourseName { get; set; } = null!;

        public string? Description { get; set; }

        public decimal? Price { get; set; }

        public decimal? DiscountPercentage { get; set; }

        public string? ThumbnailUrl { get; set; }

        public int? DurationHours { get; set; }

        public string? DifficultyLevel { get; set; }

        public bool? IsFeatured { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public CategoryModel Category { get; set; }
       
    }
}

