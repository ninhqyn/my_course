using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class Module
{
    public int ModuleId { get; set; }

    public int CourseId { get; set; }

    public string ModuleName { get; set; } = null!;

    public string? Description { get; set; }

    public int OrderIndex { get; set; }

    public int? DurationMinutes { get; set; }

    public bool? IsFree { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? LessonCount { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
