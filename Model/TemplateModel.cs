namespace MyCourse.Model
{
    public class TemplateModel
    {
        public int Id { get; set; }

        public string TemplateName { get; set; } = null!;

        public string? TemplateHtml { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
