using System;
using Contracting.Shared.Dtos;

namespace Contracting.Shared.BusinessDtos.EngineerSiteReportDto
{
    public class EngineerSiteReportFilterDto : BaseFilterDto
    {
        public Guid? ProjectId { get; set; }
    }
}
