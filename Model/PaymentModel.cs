using System;
using System.Collections.Generic;

namespace MyCourse.Model
{
    public class PaymentModel
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

        public List<PaymentDetailModel>? PaymentDetails { get; set; }
    }

    public class PaymentDetailModel
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
    }
}