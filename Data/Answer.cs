using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class Answer
{
    public int AnswerId { get; set; }

    public int QuestionId { get; set; }

    public string AnswerText { get; set; } = null!;

    public bool? IsCorrect { get; set; }

    public int? Points { get; set; }

    public string? Explanation { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Question Question { get; set; } = null!;
}
