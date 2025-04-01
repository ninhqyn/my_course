using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class UserToken
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public string? RefreshToken { get; set; }

    public bool? IsRevoked { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
