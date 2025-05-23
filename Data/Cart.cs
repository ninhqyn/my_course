﻿using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class Cart
{
    public int CartId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime? AddedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
