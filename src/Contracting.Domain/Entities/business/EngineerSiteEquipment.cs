using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class EngineerSiteEquipment : BaseAuditableEntity
    {
        public Guid EngineerSiteReportId { get; set; }
        public EngineerSiteReport EngineerSiteReport { get; set; }
        public string? name { get; set; }
        public decimal? quantity { get; set; }
        public decimal? hoursUsed { get; set; }
        public string? condition { get; set; }
        public bool? isOperational { get; set; }
        public string? notes { get; set; }
    }
}
