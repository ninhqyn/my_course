using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class Quiz
{
    public int QuizId { get; set; }

    public int ModuleId { get; set; }

    public string QuizName { get; set; } = null!;

    public string? Description { get; set; }

    public int? PassingScore { get; set; }

    public int? TimeLimitMinutes { get; set; }

    public int OrderIndex { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Module Module { get; set; } = null!;

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<QuizResult> QuizResults { get; set; } = new List<QuizResult>();
}
