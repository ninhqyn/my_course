namespace MyCourse.Model
{
    public class CategoryModel
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
        public int? ParentCategoryId { get; set; }


        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
