using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int UserId { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentStatus { get; set; }

    public string? PaymentMethod { get; set; }

    public string? TransactionId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? DiscountId { get; set; }

    public decimal? DiscountAmount { get; set; }

    public virtual Discount? Discount { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual ICollection<PaymentDetail> PaymentDetails { get; set; } = new List<PaymentDetail>();

    public virtual User User { get; set; } = null!;
}
