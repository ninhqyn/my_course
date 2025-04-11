using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace MyCourse.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet("bridge")]
        public IActionResult Bridge([FromQuery] IEnumerable<KeyValuePair<string, string>> vnpayData)
        {
            try
            {
                // Lấy các thông tin cần thiết từ query parameters
                var responseCode = vnpayData.FirstOrDefault(x => x.Key == "vnp_ResponseCode").Value;
                var orderId = vnpayData.FirstOrDefault(x => x.Key == "vnp_TxnRef").Value;

                // Tạo HTML với JavaScript để gửi message tới WebView Flutter
                string html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Payment Result</title>
    <script type='text/javascript'>
        window.onload = function() {{
            // Gửi message tới WebView Flutter
            window.flutter_inappwebview.callHandler('closeWebView', {{
                responseCode: '{responseCode}',
                orderId: '{orderId}'
            }});
        }}
    </script>
    <style>
        body {{ 
            font-family: Arial, sans-serif; 
            text-align: center; 
            padding: 20px;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
        }}
        .loader {{
            border: 4px solid #f3f3f3;
            border-top: 4px solid #3498db;
            border-radius: 50%;
            width: 40px;
            height: 40px;
            animation: spin 1s linear infinite;
            margin: 20px auto;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
    </style>
</head>
<body>
    <div>
        <div class='loader'></div>
        <h2>Loading...</h2>
        <p>Handle Loading</p>
    </div>
</body>
</html>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
             
                return Content("Error processing payment", "text/html");
            }
        }

    }
}
