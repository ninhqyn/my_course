using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class QuizResult
{
    public int ResultId { get; set; }

    public int QuizId { get; set; }

    public int UserId { get; set; }

    public decimal Score { get; set; }

    public bool? Passed { get; set; }

    public int? TimeSpentMinutes { get; set; }

    public int? AttemptNumber { get; set; }

    public DateTime? SubmissionDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
