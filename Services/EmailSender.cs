
using MailKit.Net.Smtp;
using MimeKit;
using MyCourse.IServices;
namespace MyCourse.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendVerificationCode(string toEmail, string verificationCode)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Ninh luu", "quyninh3042003@gmail.com"));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = "Mã xác thực của bạn";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px;'>
                <h2 style='color: #333;'>Xác thực tài khoản</h2>
                <p>Cảm ơn bạn đã đăng ký. Vui lòng sử dụng mã xác thực sau để hoàn tất quá trình đăng ký:</p>
                <div style='background-color: #f5f5f5; padding: 10px; text-align: center; font-size: 24px; letter-spacing: 5px; margin: 20px 0;'>
                    <b>{verificationCode}</b>
                </div>
                <p>Mã xác thực sẽ hết hạn sau 5 phút.</p>
                <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
            </div>";
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // Enable logging để xem chi tiết về quá trình kết nối
                    client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                    // Xác thực với Gmail
                    await client.AuthenticateAsync("quyninh3042003@gmail.com", "ojqg zkpu fhtl uxlh");

                    // Gửi email
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    Console.WriteLine($"Email đã được gửi thành công đến {toEmail}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gửi email: {ex.Message}");

                // Ghi chi tiết lỗi nếu có inner exception
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Chi tiết lỗi: {ex.InnerException.Message}");
                }

                throw; // Ném lại ngoại lệ để xử lý ở tầng cao hơn
            }
        }
    }
}
