using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;

namespace Contracting.Infrustructure.Inteface.business
{
    public interface IEngineerRequestAnalysisService
    {
        Task<SiteEngineerAnalysisDto> GetSiteEngineerAnalysisAsync(CancellationToken cancellationToken = default);
        Task<OfficeEngineerAnalysisDto> GetOfficeEngineerAnalysisAsync(CancellationToken cancellationToken = default);
        Task<TeamLeadAnalysisDto> GetTeamLeadAnalysisAsync(CancellationToken cancellationToken = default);
        Task<SlaBucketsReportDto> GetSlaBucketsByPriorityAndDepartmentAsync(CancellationToken cancellationToken = default);
        Task<AgingReportDto> GetAgingReportAsync(CancellationToken cancellationToken = default);
        Task<LeadCycleTimeDto> GetLeadAndCycleTimeAsync(CancellationToken cancellationToken = default);
        Task<OverdueRiskDto> GetOverdueRiskAsync(CancellationToken cancellationToken = default);
        Task<AssigneePerformanceReportDto> GetAssigneePerformanceAsync(CancellationToken cancellationToken = default);
        Task<AssigneePerformanceReportDto> GetAssigneePerformanceByEngineerIdAsync(Guid engineerId, CancellationToken cancellationToken = default);
        Task<ReworkRateDto> GetReworkRateAsync(CancellationToken cancellationToken = default);
        Task<EngineerStatusPercentageReportDto> GetRequestStatusPercentageAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<EngineerStatusPercentageReportDto> GetRequestStatusPercentageByEngineerIdAsync(Guid engineerId, int? month = null, int? year = null, CancellationToken cancellationToken = default);
        Task<WeeklyCompletionReportDto> GetWeeklyCompletionAsync(int? month = null, int? year = null, CancellationToken cancellationToken = default);
    }
}
