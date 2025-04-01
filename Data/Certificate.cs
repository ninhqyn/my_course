using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class Certificate
{
    public int CertificateId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime? IssueDate { get; set; }

    public string? CertificateUrl { get; set; }

    public string? VerificationCode { get; set; }

    public bool? IsValid { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
