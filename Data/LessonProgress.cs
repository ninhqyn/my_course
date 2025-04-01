using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class LessonProgress
{
    public int ProgressId { get; set; }

    public int LessonId { get; set; }

    public int UserId { get; set; }

    public bool? IsCompleted { get; set; }

    public int? LastPositionSeconds { get; set; }

    public int? TimeSpentMinutes { get; set; }

    public DateTime? CompletionDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
