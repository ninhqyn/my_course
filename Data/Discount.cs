using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class Discount
{
    public int DiscountId { get; set; }

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? UsageLimit { get; set; }

    public int? CurrentUsage { get; set; }

    public decimal? MinPurchaseAmount { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DiscountCourse> DiscountCourses { get; set; } = new List<DiscountCourse>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<UserDiscount> UserDiscounts { get; set; } = new List<UserDiscount>();
}
