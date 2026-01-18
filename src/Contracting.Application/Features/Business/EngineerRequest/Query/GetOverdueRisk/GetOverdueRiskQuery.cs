using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetOverdueRisk
{
    public record GetOverdueRiskQuery() : IRequest<ErrorOr<OverdueRiskDto>>;
}
