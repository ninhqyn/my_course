using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class Course
{
    public int CourseId { get; set; }

    public int? TemplateId { get; set; }

    public int? CategoryId { get; set; }

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

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public virtual ICollection<CourseInstructor> CourseInstructors { get; set; } = new List<CourseInstructor>();

    public virtual ICollection<CourseSkill> CourseSkills { get; set; } = new List<CourseSkill>();

    public virtual ICollection<DiscountCourse> DiscountCourses { get; set; } = new List<DiscountCourse>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    public virtual CertificateTemplate? Template { get; set; }
}
