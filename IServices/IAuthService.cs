using MyCourse.Model;
using MyCourse.Services;

namespace MyCourse.IServices
{
    public interface IAuthService
    {
         Task<AuthResult> SignInAsync(SignInModel model);

         Task<AuthResult> VerifyTokenAysnc(string accessToken);

        Task<AuthResult> RefreshToken(TokenModel model);

        Task<bool> SignUpAsync(SignUpModel model);
        Task<bool> VerifyCodeAsync(string email, string code);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResendVerificationCodeAsync(string email);

    }
}
