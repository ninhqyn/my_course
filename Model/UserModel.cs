namespace MyCourse.Model
{
    public class UserModel
    {
        public int UserId { get; set; }

        public string Email { get; set; } = null!;

        public string? PhoneNumber { get; set; }

      

        public string? DisplayName { get; set; }

        public string? PhotoUrl { get; set; }

        public bool? EmailVerified { get; set; }

        public bool? PhoneVerified { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
