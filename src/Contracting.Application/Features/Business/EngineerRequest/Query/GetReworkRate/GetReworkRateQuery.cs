using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetReworkRate
{
    public record GetReworkRateQuery() : IRequest<ErrorOr<ReworkRateDto>>;
}
