using MyCourse.Data;

namespace MyCourse.Model
{
    public class RatingModel
    {
        public int RatingId { get; set; }

        public int CourseId { get; set; }

        public  UserModel user { get; set; }

        public int RatingValue { get; set; }

        public string? ReviewText { get; set; }

        public bool? IsFeatured { get; set; }

        public bool? IsApproved { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
