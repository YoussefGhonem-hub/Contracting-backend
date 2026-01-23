using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class EngineerSiteReport : BaseAuditableEntity
    {
        public Guid EngineerId { get; set; }
        public Engineer Engineer { get; set; }
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }
        public DateTimeOffset ReportDate { get; set; }
        public string? GeneralNotes { get; set; }
        public string? SiteSafetyObservations { get; set; }
        public string? QualityControlObservations { get; set; }

        public ICollection<EngineerSiteWorkLog> WorkLogs { get; set; }
        public ICollection<EngineerSiteMaterial> Materials { get; set; }
        public ICollection<EngineerSiteEquipment> Equipments { get; set; }
        public ICollection<EngineerSiteSurveyQuestion> SurveyQuestions { get; set; }
    }
}
