using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class EngineerSiteSurveyQuestion : BaseAuditableEntity
    {
        public Guid EngineerSiteReportId { get; set; }
        public EngineerSiteReport EngineerSiteReport { get; set; }
        public Guid? TemplateId { get; set; }
        public EngineerSiteSurveyQuestionTemplate? Template { get; set; }
        public string? question { get; set; }
        public string? answer { get; set; }
        public string? description { get; set; }
    }
}
