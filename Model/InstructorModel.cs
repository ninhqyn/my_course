namespace MyCourse.Model
{
    public class InstructorModel
    {
        public int InstructorId { get; set; }

        public string? Bio { get; set; }

        public string? Specialization { get; set; }

        public string? WebsiteUrl { get; set; }

        public string? LinkedinUrl { get; set; }

        public bool? IsVerified { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string InstructorName { get; set; } = null!;

        public string? PhotoUrl
        {
            get; set;
        }
    }
}
