using Contracting.Shared.Dtos.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetTeamLeadAnalysis
{
    public record GetTeamLeadAnalysisQuery() : IRequest<ErrorOr<TeamLeadAnalysisDto>>;
}
