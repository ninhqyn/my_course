using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class Question
{
    public int QuestionId { get; set; }

    public int QuizId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string QuestionType { get; set; } = null!;

    public int? Points { get; set; }

    public int OrderIndex { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Quiz Quiz { get; set; } = null!;
}
