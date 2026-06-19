using Contracting.Shared.Dtos.AnalyticsDtos;
using ErrorOr;

namespace Contracting.Infrustructure.Inteface.business;

public interface IPerformanceAnalyticsService
{
    Task<ErrorOr<PerformanceAnalyticsResponseDto>> GetSiteAnalyticsAsync(
        PerformanceAnalyticsFilterDto filter,
        CancellationToken ct = default);
}
