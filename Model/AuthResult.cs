using MyCourse.Services;

namespace MyCourse.Model
{
    public class AuthResult
    {
        public AuthStatus Status { get; set; }
        public string Message { get; set; }
        public TokenModel TokenModel { get; set; }
    }
}
