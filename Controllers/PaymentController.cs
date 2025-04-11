using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using MyCourse.Model;   

namespace PaymentGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VnPayController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VnPayController> _logger;

        public VnPayController(IConfiguration configuration, ILogger<VnPayController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("create-payment")]
        public IActionResult CreateVnPayPaymentUrl([FromBody] VnPayRequest request)
        {
            try
            {
                // Lấy thông tin cấu hình từ appsettings.json
                string vnp_TmnCode = _configuration["VnPay:TmnCode"]; // Terminal ID được VNPay cấp
                string vnp_HashSecret = _configuration["VnPay:HashSecret"]; // Khóa bí mật
                string vnp_Url = _configuration["VnPay:PaymentUrl"]; // URL thanh toán của VNPay                                                
                string vnp_ReturnUrl = $"{Request.Scheme}://{Request.Host}/api/VnPay/payment-return";
      

                // Tạo thời gian hiện tại và thời gian hết hạn (hiện tại + 5 phút)
                DateTime createDate = DateTime.Now;
                DateTime expireDate = createDate.AddMinutes(5);

                // Tạo các tham số thanh toán
                Console.WriteLine(request.ReturnUrl);
                var vnp_Params = new Dictionary<string, string>
        {
            { "vnp_Version", "2.1.0" },
            { "vnp_Command", "pay" },
            { "vnp_TmnCode", vnp_TmnCode },
            { "vnp_Amount", (request.Amount * 100).ToString() }, // Số tiền x 100 (VND, không có phần thập phân)
            { "vnp_BankCode", request.BankCode ?? "" }, // Mã ngân hàng, có thể để trống để hiển thị tất cả
            { "vnp_CreateDate", createDate.ToString("yyyyMMddHHmmss") },
            { "vnp_CurrCode", "VND" },
            { "vnp_IpAddr", HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1" },
            { "vnp_Locale", request.Language ?? "vn" }, // Ngôn ngữ hiển thị: vn, en
            { "vnp_OrderInfo", request.OrderDescription },
            { "vnp_OrderType", request.OrderType ?? "other" }, // Loại hàng hóa: other, topup, billpayment, fashion...
            { "vnp_ReturnUrl", vnp_ReturnUrl},
           
            { "vnp_TxnRef", request.OrderId }, // Mã đơn hàng, không được trùng lặp trong ngày
            { "vnp_ExpireDate", expireDate.ToString("yyyyMMddHHmmss") } // Thời gian hết hạn thanh toán: 5 phút từ khi tạo
        };

                // Sắp xếp các tham số theo thứ tự a-z trước khi tạo chuỗi hash
                var sortedParams = vnp_Params.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

                // Tạo chuỗi hash data
                var queryBuilder = new StringBuilder();
                foreach (var param in sortedParams)
                {
                    if (!string.IsNullOrEmpty(param.Value))
                    {
                        queryBuilder.Append(WebUtility.UrlEncode(param.Key) + "=" + WebUtility.UrlEncode(param.Value) + "&");
                    }
                }

                // Xóa dấu & cuối cùng
                if (queryBuilder.Length > 0)
                {
                    queryBuilder.Remove(queryBuilder.Length - 1, 1);
                }

                // Tạo chuỗi hmacsha512
                var signData = queryBuilder.ToString();
                var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(vnp_HashSecret));
                var signBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
                var sign = BitConverter.ToString(signBytes).Replace("-", "").ToLower();

                // Thêm chữ ký vào chuỗi query
                queryBuilder.Append("&vnp_SecureHash=" + sign);

                // Tạo URL thanh toán hoàn chỉnh
                string paymentUrl = vnp_Url + "?" + queryBuilder.ToString();
                Console.WriteLine(vnp_ReturnUrl);
                return Ok(new { success = true, payment_url = paymentUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating VNPay payment URL");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpGet("payment-return")]
        public IActionResult PaymentReturn()
        {
            try
            {
                // Lấy các tham số từ VNPay trả về
                var vnpayData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

                // Lấy thông tin cấu hình
                string vnp_HashSecret = _configuration["VnPay:HashSecret"];
                string vnp_SecureHash = vnpayData.ContainsKey("vnp_SecureHash") ? vnpayData["vnp_SecureHash"] : string.Empty;

                // Xóa tham số chữ ký để tạo chuỗi hash mới
                if (vnpayData.ContainsKey("vnp_SecureHash"))
                {
                    vnpayData.Remove("vnp_SecureHash");
                }
                if (vnpayData.ContainsKey("vnp_SecureHashType"))
                {
                    vnpayData.Remove("vnp_SecureHashType");
                }

                // Sắp xếp lại các tham số theo thứ tự a-z
                var sortedParams = vnpayData.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

                // Tạo chuỗi hash data
                var queryBuilder = new StringBuilder();
                foreach (var param in sortedParams)
                {
                    if (!string.IsNullOrEmpty(param.Value))
                    {
                        queryBuilder.Append(WebUtility.UrlEncode(param.Key) + "=" + WebUtility.UrlEncode(param.Value) + "&");
                    }
                }

                // Xóa dấu & cuối cùng
                if (queryBuilder.Length > 0)
                {
                    queryBuilder.Remove(queryBuilder.Length - 1, 1);
                }

                // Tạo chuỗi hmacsha512
                var signData = queryBuilder.ToString();
                var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(vnp_HashSecret));
                var signBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
                var sign = BitConverter.ToString(signBytes).Replace("-", "").ToLower();

                // Kiểm tra chữ ký
                bool isValidSignature = sign == vnp_SecureHash;

                // Lấy kết quả giao dịch
                string vnp_ResponseCode = vnpayData.ContainsKey("vnp_ResponseCode") ? vnpayData["vnp_ResponseCode"] : string.Empty;
                string orderId = vnpayData.ContainsKey("vnp_TxnRef") ? vnpayData["vnp_TxnRef"] : string.Empty;
                string transactionId = vnpayData.ContainsKey("vnp_TransactionNo") ? vnpayData["vnp_TransactionNo"] : string.Empty;
                string amount = vnpayData.ContainsKey("vnp_Amount") ? (Int64.Parse(vnpayData["vnp_Amount"]) / 100).ToString() : "0";

                // Kiểm tra kết quả giao dịch
                bool isSuccessTransaction = vnp_ResponseCode == "00";

                // Tạo tham số trả về ứng dụng (dạng JSON được mã hóa URL)
                string resultParams = System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = isValidSignature && isSuccessTransaction,
                    orderId = orderId,
                    transactionId = transactionId,
                    amount = amount,
                    responseCode = vnp_ResponseCode,
                    message = GetResponseMessage(vnp_ResponseCode),
                    isValidSignature = isValidSignature
                });

                string encodedParams = WebUtility.UrlEncode(resultParams);
                Console.WriteLine("Call back Url");

                // Trả về trang HTML đơn giản với circle progress
                string html = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Xử lý giao dịch</title>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    text-align: center;
                    padding: 20px;
                    background-color: #f5f5f5;
                    color: #333;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    height: 100vh;
                    margin: 0;
                }}
                .container {{
                    max-width: 300px;
                    padding: 30px;
                    background-color: #fff;
                    border-radius: 10px;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                }}
                .spinner {{
                    width: 50px;
                    height: 50px;
                    border: 5px solid rgba(0, 0, 0, 0.1);
                    border-radius: 50%;
                    border-top-color: #3498db;
                    animation: spin 1s linear infinite;
                    margin: 0 auto 20px auto;
                }}
                @keyframes spin {{
                    to {{ transform: rotate(360deg); }}
                }}
                .message {{
                    font-size: 16px;
                    color: #555;
                }}
            </style>
            
        </head>
        <body>
            <div class='container'>
                <div class='spinner'></div>
                <div class='message'>Đang xử lý giao dịch...</div>
            </div>
        </body>
        </html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay payment return");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        // Hàm phụ trợ để lấy thông báo dựa trên mã phản hồi
        private string GetResponseMessage(string responseCode)
        {
            switch (responseCode)
            {
                case "00": return "Giao dịch thành công";
                case "07": return "Trừ tiền thành công, giao dịch bị nghi ngờ";
                case "09": return "Giao dịch thất bại do: Thẻ/Tài khoản chưa đăng ký InternetBanking";
                case "10": return "Giao dịch thất bại do: Xác thực thông tin thẻ/tài khoản không đúng quá 3 lần";
                case "11": return "Giao dịch thất bại do: Đã hết hạn chờ thanh toán";
                case "12": return "Giao dịch thất bại do: Thẻ/Tài khoản bị khóa";
                case "13": return "Giao dịch thất bại do: Nhập sai mật khẩu xác thực";
                case "24": return "Giao dịch thất bại do: Khách hàng hủy giao dịch";
                case "51": return "Giao dịch thất bại do: Tài khoản không đủ số dư";
                case "65": return "Giao dịch thất bại do: Tài khoản vượt quá hạn mức giao dịch";
                case "75": return "Ngân hàng thanh toán đang bảo trì";
                case "79": return "Giao dịch thất bại do: Nhập sai mật khẩu thanh toán nhiều lần";
                case "99": return "Lỗi không xác định";
                default: return "Lỗi không xác định";
            }
        }

        [HttpPost("ipn")]
        public IActionResult ProcessIpn()
        {
            try
            {
                Console.WriteLine("INP CALL");
                // Lấy các tham số từ VNPay IPN
                var vnpayData = Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());

                // Lấy thông tin cấu hình
                string vnp_HashSecret = _configuration["VnPay:HashSecret"];
                string vnp_SecureHash = vnpayData.ContainsKey("vnp_SecureHash") ? vnpayData["vnp_SecureHash"] : string.Empty;

                // Xóa tham số chữ ký để tạo chuỗi hash mới
                if (vnpayData.ContainsKey("vnp_SecureHash"))
                {
                    vnpayData.Remove("vnp_SecureHash");
                }
                if (vnpayData.ContainsKey("vnp_SecureHashType"))
                {
                    vnpayData.Remove("vnp_SecureHashType");
                }

                // Sắp xếp lại các tham số theo thứ tự a-z
                var sortedParams = vnpayData.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

                // Tạo chuỗi hash data
                var queryBuilder = new StringBuilder();
                foreach (var param in sortedParams)
                {
                    if (!string.IsNullOrEmpty(param.Value))
                    {
                        queryBuilder.Append(WebUtility.UrlEncode(param.Key) + "=" + WebUtility.UrlEncode(param.Value) + "&");
                    }
                }

                // Xóa dấu & cuối cùng
                if (queryBuilder.Length > 0)
                {
                    queryBuilder.Remove(queryBuilder.Length - 1, 1);
                }

                // Tạo chuỗi hmacsha512
                var signData = queryBuilder.ToString();
                var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(vnp_HashSecret));
                var signBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signData));
                var sign = BitConverter.ToString(signBytes).Replace("-", "").ToLower();

                // Kiểm tra chữ ký
                bool isValidSignature = sign == vnp_SecureHash;

                if (!isValidSignature)
                {
                    return BadRequest(new { RspCode = "97", Message = "Invalid signature" });
                }

                // Lấy kết quả giao dịch
                string vnp_ResponseCode = vnpayData.ContainsKey("vnp_ResponseCode") ? vnpayData["vnp_ResponseCode"] : string.Empty;
                string orderId = vnpayData.ContainsKey("vnp_TxnRef") ? vnpayData["vnp_TxnRef"] : string.Empty;

                // TODO: Cập nhật trạng thái đơn hàng trong database
                // Kiểm tra và cập nhật trạng thái đơn hàng trong database dựa vào vnp_ResponseCode và orderId

                // Phản hồi lại cho VNPay
                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay IPN");
                return StatusCode(500, new { RspCode = "99", Message = "Internal server error" });
            }
        }
    }

    
}