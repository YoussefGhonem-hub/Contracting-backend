using Contracting.Shared.Dtos.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetSiteEngineerAnalysis
{
    public record GetSiteEngineerAnalysisQuery() : IRequest<ErrorOr<SiteEngineerAnalysisDto>>;
}
