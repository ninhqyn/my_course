using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class UserDiscount
{
    public int UserId { get; set; }

    public int DiscountId { get; set; }

    public bool? IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Discount Discount { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
