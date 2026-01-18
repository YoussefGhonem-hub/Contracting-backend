using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetTeamLeadAnalysis
{
    public record GetTeamLeadAnalysisQuery() : IRequest<ErrorOr<TeamLeadAnalysisDto>>;
}
