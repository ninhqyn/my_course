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
    }
}
