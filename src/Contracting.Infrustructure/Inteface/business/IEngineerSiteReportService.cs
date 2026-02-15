using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;

namespace Contracting.Infrustructure.Inteface.business
{
    public interface IEngineerSiteReportService
    {
        Task<GetEngineerSiteReportDto> CreateEngineerSiteReportAsync(CreateEngineerSiteReportDto dto);
        Task<GetEngineerSiteReportDto> GetEngineerSiteReportByIdAsync(Guid reportId);
        Task<PaginatedList<GetEngineerSiteReportDto>> GetMyEngineerSiteReportsAsync(EngineerSiteReportFilterDto filter, CancellationToken cancellationToken = default);
        Task<PaginatedList<GetEngineerSiteReportDto>> GetEngineerSiteReportsByEngineerIdAsync(Guid engineerId, EngineerSiteReportFilterDto filter, CancellationToken cancellationToken = default);
        Task<PaginatedList<GetEngineerSiteReportDto>> GetAllEngineerSiteReportsAsync(EngineerSiteReportFilterDto filter, CancellationToken cancellationToken = default);
    }
}
