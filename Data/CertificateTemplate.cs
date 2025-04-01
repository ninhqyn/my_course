using System;
using System.Collections.Generic;

namespace MyCourse.Data;

public partial class CertificateTemplate
{
    public int Id { get; set; }

    public string TemplateName { get; set; } = null!;

    public string? TemplateHtml { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
