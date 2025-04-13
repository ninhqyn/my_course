namespace MyCourse.IServices
{
    public interface IEmailSender
    {
        Task SendVerificationCode(string toEmail, string verificationCode);
    }
}
