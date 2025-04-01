using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class Skill
{
    public int SkillId { get; set; }

    public string SkillName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CourseSkill> CourseSkills { get; set; } = new List<CourseSkill>();
}
