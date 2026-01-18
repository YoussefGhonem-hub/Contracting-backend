using Contracting.Shared.Dtos.BusinessDtos.EngineerRequestAnalysisDtos;

namespace Contracting.Infrustructure.Inteface.business
{
    public interface IEngineerRequestAnalysisService
    {
        Task<SiteEngineerAnalysisDto> GetSiteEngineerAnalysisAsync(CancellationToken cancellationToken = default);
        Task<OfficeEngineerAnalysisDto> GetOfficeEngineerAnalysisAsync(CancellationToken cancellationToken = default);
        Task<TeamLeadAnalysisDto> GetTeamLeadAnalysisAsync(CancellationToken cancellationToken = default);
    }
}
