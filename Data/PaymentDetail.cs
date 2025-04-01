using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class PaymentDetail
{
    public int Id { get; set; }

    public int PaymentId { get; set; }

    public string ItemType { get; set; } = null!;

    public int ItemId { get; set; }

    public int? Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal Subtotal { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Payment Payment { get; set; } = null!;
}
