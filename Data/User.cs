using System;
using System.Collections.Generic;
namespace MyCourse.Data;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string? PhotoUrl { get; set; }

    public bool? EmailVerified { get; set; }

    public bool? PhoneVerified { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    
    public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual ICollection<UserAuthentication> UserAuthentications { get; set; } = new List<UserAuthentication>();

    public virtual ICollection<UserDiscount> UserDiscounts { get; set; } = new List<UserDiscount>();

    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
}
