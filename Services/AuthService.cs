
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using MyCourse.Data;
using MyCourse.IServices;
using MyCourse.Model;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyCourse.Services
{
    public enum AuthStatus
    {
        Success,
        UserNotFound,
        InvalidPassword,
        EmailRequired,
        TokenExpired,
        InvalidToken
    }
    public class AuthService : IAuthService
    {
        private readonly MyCourseContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _memoryCache;
        public AuthService(MyCourseContext context, IConfiguration configuration,IEmailSender emailSender, IMemoryCache memoryCache)
        {   
            _context = context;
            _configuration = configuration;
            _emailSender = emailSender;
            _memoryCache = memoryCache;
        }

        public async Task<AuthResult> SignInAsync(SignInModel model)
        {
            // Check if email is empty or null
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                return new AuthResult
                {
                    Status = AuthStatus.EmailRequired,
                    Message = "Email is required."
                };
            }

            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == model.Email);

            // Check if user exists
            if (user == null)
            {
                return new AuthResult
                {
                    Status = AuthStatus.UserNotFound,
                    Message = "User not found."
                };
            }

            // Check password
            // Simple password comparison (replace with secure hashing in production)
            if (user.PasswordHash != model.Password)
            {
                return new AuthResult
                {
                    Status = AuthStatus.InvalidPassword,
                    Message = "Invalid password."
                };
            }

            // Generate token
            var token = GenerateToken(user);
          
          
            var newRefreshTokenEntity = new UserToken
            {
                UserId = user.UserId,
                Token = token.AccessToken,
                RefreshToken = token.RefreshToken,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            // Thêm RefreshToken mới vào cơ sở dữ liệu
            _context.UserTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();
            return new AuthResult
            {
                Status = AuthStatus.Success,
                Message = "Login successful.",
                TokenModel = token
            };
        }

        private TokenModel GenerateToken(User user)
        {
            // Generate JWT token logic
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
               new Claim("UserId",user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );

            return new TokenModel
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = GenerateRefreshToken()
            };
        }

        private string GenerateRefreshToken()
        {
            // Generate a secure refresh token 
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }


        public async Task<AuthResult> RefreshToken(TokenModel model)
        {
            // Kiểm tra xem RefreshToken có hợp lệ không và chưa bị thu hồi
            var refreshTokenEntity = await _context.UserTokens
                .FirstOrDefaultAsync(rt => rt.RefreshToken == model.RefreshToken && rt.IsRevoked != true);

            if (refreshTokenEntity == null)
            {
                return new AuthResult
                {
                    Status = AuthStatus.InvalidToken,
                    Message = "Invalid or revoked refresh token."
                };
            }

            // Kiểm tra xem RefreshToken có hết hạn không
            if (refreshTokenEntity.ExpiresAt < DateTime.UtcNow)
            {
                return new AuthResult
                {
                    Status = AuthStatus.TokenExpired,
                    Message = "Refresh token has expired."
                };
            }

            // Kiểm tra AccessToken
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jsonToken = handler.ReadToken(model.AccessToken) as JwtSecurityToken;

                // Chỉ refresh token khi AccessToken đã hết hạn
                if (jsonToken.ValidTo > DateTime.UtcNow)
                {
                    return new AuthResult
                    {
                        Status = AuthStatus.Success,
                        Message = "Access token is still valid.",
                        TokenModel = model
                    };
                }
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    Status = AuthStatus.InvalidToken,
                    Message = $"Error validating access token: {ex.Message}"
                };
            }

            // Lấy thông tin người dùng từ RefreshToken
            var user = await _context.Users.FindAsync(refreshTokenEntity.UserId);
            if (user == null)
            {
                return new AuthResult
                {
                    Status = AuthStatus.UserNotFound,
                    Message = "User not found."
                };
            }

            // Tạo lại AccessToken mới
            var newToken = GenerateToken(user);

            // Đánh dấu RefreshToken cũ là đã bị thu hồi
            refreshTokenEntity.IsRevoked = true;
            _context.UserTokens.Update(refreshTokenEntity);
            await _context.SaveChangesAsync();

            var newRefreshTokenEntity = new UserToken
            {
                UserId = user.UserId,
                Token = newToken.AccessToken,
                RefreshToken = newToken.RefreshToken,
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            // Thêm RefreshToken mới vào cơ sở dữ liệu
            _context.UserTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            // Trả về AccessToken mới và RefreshToken mới
            return new AuthResult
            {
                Status = AuthStatus.Success,
                Message = "Token refreshed successfully.",
                TokenModel = newToken
            };
        }

       
        public async Task<AuthResult> VerifyTokenAysnc(string accessToken)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(accessToken) as JwtSecurityToken;

                if (jsonToken == null)
                {
                    return new AuthResult
                    {
                        Status = AuthStatus.InvalidToken,
                        Message = "Invalid token format."
                    };
                }

                // Kiểm tra chữ ký của token và xác minh token hợp lệ
                var key = Encoding.UTF8.GetBytes(_configuration["JWT:Key"]);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                var principal = handler.ValidateToken(accessToken, validationParameters, out var validatedToken);

                // Kiểm tra thời gian hết hạn của token
                var expirationTime = validatedToken.ValidTo;

                // So sánh thời gian hết hạn với thời gian hiện tại
                if (expirationTime > DateTime.UtcNow)
                {
                    // Token còn hiệu lực
                    return new AuthResult
                    {
                        Status = AuthStatus.Success,
                        Message = "Token is valid.",
                        TokenModel = new TokenModel { AccessToken = accessToken }
                    };
                }

                // Token đã hết hạn
                return new AuthResult
                {
                    Status = AuthStatus.TokenExpired,
                    Message = "Token has expired."
                };
            }
            catch (Exception ex)
            {
                // Xử lý lỗi (token không hợp lệ, định dạng sai, v.v.)
                return new AuthResult
                {
                    Status = AuthStatus.InvalidToken,
                    Message = $"Error verifying token: {ex.Message}"
                };
            }
        }
        private async Task SendVerificationEmail(string email, string verificationCode)
        {
            try
            {
                await _emailSender.SendVerificationCode(email, verificationCode);
            }
            catch (Exception ex)
            {
                // Log lỗi và xử lý
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw;
            }
        }
        private string GenerateRandomVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // Generates a 6-digit code
        }

        public async Task<bool> SignUpAsync(SignUpModel model)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email || u.DisplayName == model.UserName);

            if (existingUser == null)
            {
                var verificationCode = GenerateRandomVerificationCode();

                // Lưu tạm thông tin người dùng vào memory cache
                var cacheKey = $"verification_{model.Email}";
                _memoryCache.Set(cacheKey, (verificationCode, DateTime.UtcNow.AddMinutes(5), model));

                Console.WriteLine($"Stored verification code for {model.Email}: {verificationCode}");

                // Gửi email xác thực
                await SendVerificationEmail(model.Email, verificationCode);
                return true;
            }

            return false;
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            Console.WriteLine("input code:" + code);
            var cacheKey = $"verification_{email}";

            // Kiểm tra mã trong memory cache
            if (_memoryCache.TryGetValue(cacheKey, out (string VerificationCode, DateTime Expiry, SignUpModel Model) value))
            {
                Console.WriteLine("input send:" + value.VerificationCode);
                // Kiểm tra mã và thời gian hết hạn
                if (value.VerificationCode == code && DateTime.UtcNow <= value.Expiry)
                {
                    var model = value.Model;
                    // Tạo người dùng trong cơ sở dữ liệu
                    //var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    var user = new User
                    {
                        DisplayName = model.UserName,
                        PasswordHash = model.Password,
                        Email = email,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        
                    };

                    await _context.AddAsync(user);
                    var result = await _context.SaveChangesAsync();
                    Console.WriteLine($"Saved {result} changes");
                    // Xóa khỏi cache sau khi đăng ký thành công
                    _memoryCache.Remove(cacheKey);
                    return true;
                }
            }

            return false; // Mã xác thực không hợp lệ hoặc đã hết hạn
        }
        public async Task<bool> ResendVerificationCodeAsync(string email)
        {
            var cacheKey = $"verification_{email}";
            if (_memoryCache.TryGetValue(cacheKey, out (string VerificationCode, DateTime Expiry, SignUpModel Model) value))
            {
                var newVerificationCode = GenerateRandomVerificationCode();
                var newExpiry = DateTime.UtcNow.AddMinutes(5);

                // Cập nhật mã xác thực và thời gian hết hạn
                _memoryCache.Set(cacheKey, (newVerificationCode, DateTime.UtcNow.AddMinutes(5), value.Model));

                // Gửi mã xác thực mới qua email
                await SendVerificationEmail(email, newVerificationCode);
                return true;
            }

            return false; // Email không tồn tại trong danh sách chờ
        }
        public Task<bool> ForgotPasswordAsync(string email)
        {
            throw new NotImplementedException();
        }
    }

   
   
}