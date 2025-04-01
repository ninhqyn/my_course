using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

public static class TokenHelper
{
    // Hàm lấy UserId từ token
    public static int GetUserIdFromToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return 0; // Trả về 0 nếu token rỗng hoặc null
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jsonToken == null)
            {
                return 0; // Trả về 0 nếu không thể đọc token
            }

            // Tìm claim với tên "UserId"
            var userIdClaim = jsonToken?.Claims.FirstOrDefault(c => c.Type == "UserId");

            if (userIdClaim == null)
            {
                return 0; // Trả về 0 nếu không tìm thấy claim "UserId"
            }

            // Chuyển đổi giá trị của claim "UserId" thành int và trả về
            return int.TryParse(userIdClaim.Value, out var userId) ? userId : 0;
        }
        catch
        {
            return 0; // Trả về 0 nếu có lỗi trong quá trình giải mã token
        }
    }
}
