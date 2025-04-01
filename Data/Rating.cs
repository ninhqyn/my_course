using System;
using System.Collections.Generic;
namespace MyCourse.Data;

public partial class Rating
{
    public int RatingId { get; set; }

    public int CourseId { get; set; }

    public int UserId { get; set; }

    public int RatingValue { get; set; }

    public string? ReviewText { get; set; }

    public bool? IsFeatured { get; set; }

    public bool? IsApproved { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
