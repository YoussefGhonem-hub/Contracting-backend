using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestStatusPercentageByEngineerId
{
    public record GetRequestStatusPercentageByEngineerIdQuery(Guid EngineerId, int? Month = null, int? Year = null) : IRequest<ErrorOr<EngineerStatusPercentageReportDto>>;
}
