using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestStatusPercentageByEngineerId
{
    public record GetRequestStatusPercentageByEngineerIdQuery(Guid EngineerId, DateTime? StartDate = null, DateTime? EndDate = null) : IRequest<ErrorOr<EngineerStatusPercentageReportDto>>;
}
