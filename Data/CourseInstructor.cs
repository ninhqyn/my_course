using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class CourseInstructor
{
    public int CourseId { get; set; }

    public int InstructorId { get; set; }

    public string? Role { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Instructor Instructor { get; set; } = null!;
}
