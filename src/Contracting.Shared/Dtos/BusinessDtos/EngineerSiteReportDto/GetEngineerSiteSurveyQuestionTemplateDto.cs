namespace Contracting.Shared.BusinessDtos.EngineerSiteReportDto
{
    public class GetEngineerSiteSurveyQuestionTemplateDto
    {
        public Guid Id { get; set; }
        public string? question { get; set; }
        public int order { get; set; }
        public bool? isActive { get; set; }
    }
}
