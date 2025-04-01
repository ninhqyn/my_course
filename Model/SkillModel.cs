namespace MyCourse.Model
{
    public class SkillModel
    {
        public int SkillId { get; set; }

        public string SkillName { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
