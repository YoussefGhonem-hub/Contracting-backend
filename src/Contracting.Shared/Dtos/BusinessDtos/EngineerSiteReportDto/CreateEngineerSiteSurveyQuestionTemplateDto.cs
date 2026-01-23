namespace Contracting.Shared.BusinessDtos.EngineerSiteReportDto
{
    public class CreateEngineerSiteSurveyQuestionTemplateDto
    {
        public string? question { get; set; }
        public int order { get; set; }
        public bool isActive { get; set; } = true;
    }

    public class UpdateEngineerSiteSurveyQuestionTemplateDto
    {
        public Guid Id { get; set; }
        public string? question { get; set; }
        public int order { get; set; }
        public bool isActive { get; set; } = true;
    }
}
