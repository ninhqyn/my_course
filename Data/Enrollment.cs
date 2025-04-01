using System;
using System.Collections.Generic;
namespace MyCourse.Data;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public int? PaymentId { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool? IsActive { get; set; }

    public decimal? ProgressPercentage { get; set; }

    public DateTime? CompletionDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Payment? Payment { get; set; }

    public virtual User User { get; set; } = null!;
}
