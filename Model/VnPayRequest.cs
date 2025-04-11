namespace MyCourse.Model
{
    public class VnPayRequest
    {
        public string OrderId { get; set; } // Mã đơn hàng
        public long Amount { get; set; } // Số tiền thanh toán (VND)
        public string OrderDescription { get; set; } // Mô tả đơn hàng
        public string BankCode { get; set; } // Mã ngân hàng (có thể null)
        public string Language { get; set; } // Ngôn ngữ hiển thị (vn, en)
        public string OrderType { get; set; } // Loại hàng hóa

        public string ReturnUrl { get; set; }
    }
}
