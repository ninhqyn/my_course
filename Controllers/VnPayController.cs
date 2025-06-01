using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using MyCourse.Model;
using MyCourse.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MyCourse.IServices;
using System.Runtime.ConstrainedExecution;

namespace PaymentGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VnPayController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VnPayController> _logger;
        private readonly MyCourseContext _context;
        private readonly ICartService _cartService;

        public VnPayController(IConfiguration configuration, ILogger<VnPayController> logger, MyCourseContext context,ICartService cartService)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _cartService = cartService;
        }

        [HttpPost("create-payment")]
        [Authorize]
        public IActionResult CreateVnPayPaymentUrl([FromBody] VnPayRequest request)
        {
            try
            {
                var userId = TokenHelper.GetUserIdFromToken(Request.Headers["Authorization"].ToString()?.Replace("Bearer ", ""));

                if (userId == 0)
                {
                    return Unauthorized(new { message = "Invalid token" });
                }
                // Lấy thông tin cấu hình từ appsettings.json
                string vnp_TmnCode = _configuration["VnPay:TmnCode"];
                string vnp_HashSecret = _configuration["VnPay:HashSecret"];
                string vnp_Url = _configuration["VnPay:PaymentUrl"];
                string vnp_ReturnUrl = $"{Request.Scheme}://{Request.Host}/api/VnPay/payment-return";

                // Tạo thời gian hiện tại và thời gian hết hạn (hiện tại + 5 phút)
                DateTime createDate = DateTime.Now;
                DateTime expireDate = createDate.AddMinutes(5);


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

                // Store course IDs in session or database for later retrieval
                // For this example, let's add it to the order info as JSON
                string orderInfoWithCourseIds = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Description = request.OrderDescription,
                    Courses = request.Courses,
                    UserId = userId
                });
                Console.WriteLine("user id:" + userId);
                foreach (var courseId in request.Courses)
                {
                    Console.WriteLine($"- Course ID: {courseId}");
                 
                }
                vnp_Params["vnp_OrderInfo"] = orderInfoWithCourseIds;

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
        public async Task<IActionResult> PaymentReturn()
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
                string amountStr = vnpayData.ContainsKey("vnp_Amount") ? (Int64.Parse(vnpayData["vnp_Amount"]) / 100).ToString() : "0";
                decimal amount = decimal.Parse(amountStr);
                string orderInfo = vnpayData.ContainsKey("vnp_OrderInfo") ? vnpayData["vnp_OrderInfo"] : string.Empty;

                // Parse order info to extract user ID and course IDs
                int userId = 0;
                List<int> courseIds = new List<int>();
                string description = "";

                try
                {
                    using JsonDocument doc = JsonDocument.Parse(orderInfo);
                    JsonElement root = doc.RootElement;

                    if (root.TryGetProperty("UserId", out JsonElement userIdElement) && userIdElement.ValueKind == JsonValueKind.Number)
                        userId = userIdElement.GetInt32();

                    if (root.TryGetProperty("Courses", out JsonElement coursesElement) && coursesElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement courseElement in coursesElement.EnumerateArray())
                        {
                            if (courseElement.ValueKind == JsonValueKind.Number)
                            {
                                courseIds.Add(courseElement.GetInt32());
                            }
                        }
                    }

                    if (root.TryGetProperty("Description", out JsonElement descElement) && descElement.ValueKind == JsonValueKind.String)
                        description = descElement.GetString() ?? "";

                    _logger.LogInformation($"Parsed order info: UserId={userId}, Courses={string.Join(",", courseIds)}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing order info");
                }

                // Kiểm tra kết quả giao dịch
                bool isSuccessTransaction = vnp_ResponseCode == "00";

                // Create Payment and Enrollments if transaction is successful
                if (isValidSignature && isSuccessTransaction && courseIds.Count > 0)
                {
                    try
                    {
                        // Create new payment record
                        var payment = new Payment
                        {
                            UserId = userId,
                            Amount = amount,
                            PaymentStatus = "Completed",
                            PaymentMethod = "VNPay",
                            TransactionId = transactionId,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        };

                        await _context.Payments.AddAsync(payment);
                        await _context.SaveChangesAsync();

                        // Calculate unit price per course
                        decimal unitPrice = courseIds.Count > 0 ? amount / courseIds.Count : amount;

                        foreach (var courseId in courseIds)
                        {
                            // Create payment detail for each course
                            var paymentDetail = new PaymentDetail
                            {
                                PaymentId = payment.PaymentId,
                                ItemType = "Course",
                                ItemId = courseId,
                                Quantity = 1,
                                UnitPrice = unitPrice,
                                DiscountAmount = 0,
                                Subtotal = unitPrice,
                                CreatedAt = DateTime.Now
                            };

                            await _context.PaymentDetails.AddAsync(paymentDetail);

                            // Create enrollment for each course
                            var enrollment = new Enrollment
                            {
                                UserId = userId,
                                CourseId = courseId,
                                EnrollmentDate = DateTime.Now,
                                PaymentId = payment.PaymentId,
                                IsActive = true
                            };

                            await _context.Enrollments.AddAsync(enrollment);
                           
                        }
                        var result = await _cartService.ClearCart(userId);

                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Payment and enrollments created successfully for user {userId} for courses {string.Join(",", courseIds)}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating payment and enrollment records");
                    }
                }

                // Tạo tham số trả về ứng dụng (dạng JSON được mã hóa URL)
                string resultParams = System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = isValidSignature && isSuccessTransaction,
                    orderId = orderId,
                    transactionId = transactionId,
                    amount = amountStr,
                    responseCode = vnp_ResponseCode,
                    message = GetResponseMessage(vnp_ResponseCode),
                    isValidSignature = isValidSignature
                });

                string encodedParams = WebUtility.UrlEncode(resultParams);
                _logger.LogInformation("Payment return processed: " + resultParams);

                // Create return URL with frontend app
                string returnUrl = _configuration["App:FrontendUrl"] + "/payment/result?" + encodedParams;
                Console.WriteLine(returnUrl);
                // Trả về trang HTML đơn giản với circle progress và JavaScript redirect
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
            <script>
                // Redirect to frontend after a short delay
                setTimeout(function() {{
                    window.location.href = '{returnUrl}';
                }}, 2000);
            </script>
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
    }
}