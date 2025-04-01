using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class CourseSkill
{
    public int CourseId { get; set; }

    public int SkillId { get; set; }

    public string? ProficiencyLevel { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Skill Skill { get; set; } = null!;
}
