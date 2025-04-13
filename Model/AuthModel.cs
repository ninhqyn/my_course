namespace MyCourse.Model
{
    public class AuthModel
    {
    }
    public class VerifyCodeModel
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }
    public class ForgotPasswordModel
    {
        public string Email { get; set; }
    }

    public class ResendVerificationModel
    {
        public string Email { get; set; }
    }

}
