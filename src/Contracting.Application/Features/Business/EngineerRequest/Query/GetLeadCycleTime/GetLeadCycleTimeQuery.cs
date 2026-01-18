using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetLeadCycleTime
{
    public record GetLeadCycleTimeQuery() : IRequest<ErrorOr<LeadCycleTimeDto>>;
}
