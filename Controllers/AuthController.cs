using Microsoft.AspNetCore.Mvc;
using MyCourse.IServices;
using MyCourse.Model;
using MyCourse.Services;

namespace MyCourse.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Login Endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] SignInModel model)
        {
            var authResult = await _authService.SignInAsync(model);

            return authResult.Status switch
            {
                AuthStatus.Success => Ok(authResult.TokenModel),
                AuthStatus.UserNotFound => NotFound(new { message = authResult.Message }),
                AuthStatus.InvalidPassword => Unauthorized(new { message = authResult.Message }),
                AuthStatus.EmailRequired => BadRequest(new { message = authResult.Message }),
                _ => StatusCode(500, new { message = "An unexpected error occurred." })
            };
        }

     
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel model)
        {
            var authResult = await _authService.RefreshToken(model);

            return authResult.Status switch
            {
                AuthStatus.Success => Ok(authResult.TokenModel),
                AuthStatus.InvalidToken => Unauthorized(new { message = authResult.Message }),
                AuthStatus.TokenExpired => Unauthorized(new { message = authResult.Message }),
                _ => StatusCode(500, new { message = "An unexpected error occurred." })
            };
        }

        [HttpPost("verify-token")]
        public async Task<IActionResult> VerifyToken([FromBody] VerifyModel model)
        {
            var authResult = await _authService.VerifyTokenAysnc(model.accessToken);

            return authResult.Status switch
            {
                AuthStatus.Success => Ok(new { message = authResult.Message, token = model.accessToken }),
                AuthStatus.InvalidToken => Unauthorized(new { message = authResult.Message }),
                AuthStatus.TokenExpired => Unauthorized(new { message = authResult.Message }),
                _ => StatusCode(500, new { message = "An unexpected error occurred." })
            };
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] SignUpModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.SignUpAsync(model);
            if (result)
            {
                return Ok(new { message = "Vui lòng kiểm tra email để xác thực tài khoản" });
            }

            return BadRequest(new { message = "Email hoặc tên người dùng đã tồn tại" });
        }

        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Code))
            {
                return BadRequest(new { message = "Email và mã xác thực không được để trống" });
            }

            var result = await _authService.VerifyCodeAsync(model.Email, model.Code);
            if (result)
            {
                return Ok(new { message = "Xác thực tài khoản thành công" });
            }

            return BadRequest(new { message = "Mã xác thực không hợp lệ hoặc đã hết hạn" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(new { message = "Email không được để trống" });
            }

            var result = await _authService.ForgotPasswordAsync(model.Email);
            if (result)
            {
                return Ok(new { message = "Hướng dẫn đặt lại mật khẩu đã được gửi đến email của bạn" });
            }

            return NotFound(new { message = "Không tìm thấy tài khoản với email này" });
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationModel model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(new { message = "Email không được để trống" });
            }

            var result = await _authService.ResendVerificationCodeAsync(model.Email);
            if (result)
            {
                return Ok(new { message = "Mã xác thực mới đã được gửi đến email của bạn" });
            }

            return BadRequest(new { message = "Không thể gửi lại mã xác thực. Vui lòng thử lại sau." });
        }
    }
}
