using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Dtos;

namespace Contracting.Infrustructure.Inteface.business
{
    public interface IEngineerSiteReportService
    {
        Task<GetEngineerSiteReportDto> CreateEngineerSiteReportAsync(CreateEngineerSiteReportDto dto);
        Task<GetEngineerSiteReportDto> GetEngineerSiteReportByIdAsync(Guid reportId);
        Task<List<GetEngineerSiteReportDto>> GetMyEngineerSiteReportsAsync(BaseFilterDto filter, CancellationToken cancellationToken = default);
        Task<PaginatedList<GetEngineerSiteReportDto>> GetEngineerSiteReportsByEngineerIdAsync(Guid engineerId, BaseFilterDto filter, CancellationToken cancellationToken = default);
    }
}
