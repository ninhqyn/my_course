using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class Lesson
{
    public int LessonId { get; set; }

    public int ModuleId { get; set; }

    public string LessonName { get; set; } = null!;

    public string? Content { get; set; }

    public string? VideoUrl { get; set; }

    public int? DurationMinutes { get; set; }

    public int OrderIndex { get; set; }

    public bool? IsPreview { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

    public virtual Module Module { get; set; } = null!;
}
