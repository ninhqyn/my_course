namespace MyCourse.Model
{
    public class CartModel
    {
        public int CartId { get; set; }

        public int UserId { get; set; }

        public CourseModel Course { get; set; }

        public DateTime? AddedAt { get; set; }

        public bool? IsActive { get; set; }
    }
}
