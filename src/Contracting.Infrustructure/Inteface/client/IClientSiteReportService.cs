using Contracting.Shared.Dtos.ClientDtos.ReportDtos;

namespace Contracting.Infrustructure.Inteface.client;

public interface IClientSiteReportService
{
    Task<List<GetClientSiteReportListItemDto>?> GetClientSiteReportsAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<GetClientSiteReportDetailDto?> GetClientSiteReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default);
}
