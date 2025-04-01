using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class UserAuthentication
{
    public int AuthId { get; set; }

    public int UserId { get; set; }

    public string ProviderName { get; set; } = null!;

    public string? ProviderId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
