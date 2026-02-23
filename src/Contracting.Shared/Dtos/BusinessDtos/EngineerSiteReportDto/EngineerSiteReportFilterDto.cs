using System;
using Contracting.Shared.Dtos;

namespace Contracting.Shared.BusinessDtos.EngineerSiteReportDto
{
    public class EngineerSiteReportFilterDto : BaseFilterDto
    {
        public Guid? ProjectId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
