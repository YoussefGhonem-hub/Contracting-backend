using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class EngineerSiteSurveyQuestionTemplate : BaseAuditableEntity
    {
        public string? question { get; set; }
        public int order { get; set; }
        public bool isActive { get; set; } = true;
    }
}
